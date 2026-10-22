
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.Json;
using Avalonia.Platform;
using IMoRS.DTOs;

namespace IMoRS.Services;

public static class DatabaseBackupService
{
    private static string AssetsFolder => "Assets/SignIconPng";

    private static string? FindAssetUri(string? iconPath)
    {
        if (string.IsNullOrWhiteSpace(iconPath))
            return null;

        if (iconPath.StartsWith(
                "avares://",
                StringComparison.OrdinalIgnoreCase))
        {
            return iconPath;
        }

        // Ищем ресурс по имени файла, если IconPath — временный путь.
        try
        {
            var assemblyName = Assembly.GetExecutingAssembly()
                .GetName().Name;

            if (string.IsNullOrWhiteSpace(assemblyName))
                return null;

            var folderUri = new Uri(
                $"avares://{assemblyName}/{AssetsFolder}");

            var fileName = Path.GetFileName(iconPath);

            foreach (var assetUri in AssetLoader.GetAssets(folderUri, null))
            {
                if (string.Equals(
                        Path.GetFileName(assetUri.AbsolutePath),
                        fileName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return assetUri.ToString();
                }
            }
        }
        catch
        {
            // Если ресурс не найден, ниже попробуем прочитать файл.
        }

        return null;
    }

    public static async Task ExportAsync(
        string filePath,
        IEnumerable<MarkerDto> markers)
    {
        var exportList = new List<MarkerExportDto>();

        foreach (var marker in markers)
        {
            string iconData = "";
            string? iconExtension = null;

            var assetUri = marker.IsCustomIcon
                ? null
                : FindAssetUri(marker.IconPath);

            if (!marker.IsCustomIcon && assetUri != null)
            {
                // Стандартная иконка: сохраняем URI ресурса.
                iconData = assetUri;
            }
            else if (!string.IsNullOrWhiteSpace(marker.IconPath) &&
                     File.Exists(marker.IconPath))
            {
                // Пользовательская иконка либо временная копия
                // стандартной: сохраняем содержимое файла.
                try
                {
                    var bytes = await File.ReadAllBytesAsync(
                        marker.IconPath);

                    iconData = Convert.ToBase64String(bytes);
                    iconExtension = Path.GetExtension(marker.IconPath);
                }
                catch (IOException)
                {
                    // Не прерываем экспорт остальных меток.
                }
                catch (UnauthorizedAccessException)
                {
                    // Не прерываем экспорт остальных меток.
                }
            }

            exportList.Add(new MarkerExportDto
            {
                X = marker.X,
                Y = marker.Y,
                Scale = marker.Scale,
                Description = marker.Description,
                IconBase64 = iconData,
                IconExtension = iconExtension,
                IsCustomIcon = marker.IsCustomIcon
            });
        }

        var json = JsonSerializer.Serialize(
            exportList,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        var fullPath = Path.GetFullPath(filePath);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(fullPath, json);
    }

    public static async Task<List<MarkerDto>> ImportAsync(
        string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);

        var importedDtos =
            JsonSerializer.Deserialize<List<MarkerExportDto>>(json)
            ?? new List<MarkerExportDto>();

        var resultList = new List<MarkerDto>();
        var iconsFolder = IconStorageService.UserIconsDir;

        Directory.CreateDirectory(iconsFolder);

        foreach (var dto in importedDtos)
        {
            string iconPath = "";
            bool isCustomIcon = dto.IsCustomIcon;

            if (!string.IsNullOrWhiteSpace(dto.IconBase64) &&
                dto.IconBase64.StartsWith(
                    "avares://",
                    StringComparison.OrdinalIgnoreCase))
            {
                // Стандартная иконка из ресурсов приложения.
                iconPath = dto.IconBase64;
                isCustomIcon = false;
            }
            else if (!string.IsNullOrWhiteSpace(dto.IconBase64))
            {
                // Base64 — изображение, сохранённое в JSON.
                var imageBytes =
                    Convert.FromBase64String(dto.IconBase64);

                var extension = dto.IconExtension;

                if (string.IsNullOrWhiteSpace(extension) ||
                    !extension.StartsWith('.') ||
                    extension.IndexOfAny(
                        Path.GetInvalidFileNameChars()) >= 0 ||
                    extension.Contains('/') ||
                    extension.Contains('\\'))
                {
                    extension = ".png";
                }

                var fileName = Guid.NewGuid() + extension;
                iconPath = Path.Combine(iconsFolder, fileName);

                await File.WriteAllBytesAsync(
                    iconPath,
                    imageBytes);

                // Если стандартный ресурс был недоступен при экспорте,
                // его изображение тоже восстанавливается из Base64.
                isCustomIcon = true;
            }

            resultList.Add(new MarkerDto
            {
                X = dto.X,
                Y = dto.Y,
                Scale = dto.Scale,
                Description = dto.Description,
                IconPath = iconPath,
                IsCustomIcon = isCustomIcon
            });
        }

        return resultList;
    }
}
