using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using IMoRS.DTOs;
using IMoRS.Services;

namespace IMoRS.ViewModels;

public partial class MainWindowViewModel
{
    /// <summary>Создает и настраивает карту с загруженными метками.</summary>
    private void CreateMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        
        Map = map;
        
        Map.Navigator.ViewportChanged += OnViewportChanged;
        
        _markerLayer.Name = "Markers";
        _markerLayer.Features = new List<IFeature>();
        
        LoadMarkersFromDb();
        
        map.Layers.Add(_markerLayer);
        
        var state = MapStateService.Load();
        var settings = SettingsService.Load();
        
        double centerX, centerY, resolution;
        
        if (settings.RestoreMapState
            && state != null
            && IsFinite(state.X) && IsFinite(state.Y)
            && IsFinite(state.Resolution) && state.Resolution > 0)
        {
            centerX = state.X;
            centerY = state.Y;
            resolution = state.Resolution;
        }
        else
        {
            (centerX, centerY) = SphericalMercator.FromLonLat(
                settings.DefaultMapX,
                settings.DefaultMapY);
            resolution = 156543.03392804097 / Math.Pow(2, settings.DefaultMapZoom);
        }
        
        map.Navigator.CenterOn(centerX, centerY);
        map.Navigator.ZoomTo(resolution);

        Map = map;
        Map.Refresh();   
    }
    
    private static bool IsFinite(double v)
        => !double.IsNaN(v) && !double.IsInfinity(v);


    private void RemovePendingMarkerLayer()
    {
        if (Map == null) return;
        var layer = Map.Layers.FirstOrDefault(x => x.Name == "PendingMarker");
        if (layer != null)
        {
            Map.Layers.Remove(layer);
            Map.Refresh();
        }
    }
    
    private void OnViewportChanged(object? sender, ViewportChangedEventArgs e)
    {
        if (Map == null || _markerLayer == null) return;

        double currentResolution = Map.Navigator.Viewport.Resolution;
    
        double currentZoomLevel = Math.Log2(156543.03392804097 / currentResolution);
        
        double baseZoomLevel = 15.0; 

        double zoomDiff = currentZoomLevel - baseZoomLevel;

        double scaleFactor = Math.Pow(1.2, zoomDiff); 
    
        scaleFactor = Math.Clamp(scaleFactor, 0.2, 5.0);

        foreach (var feature in _markers)
        {
            if (feature["Marker"] is MarkerDto dto)
            {
                feature.Styles.Clear();
                feature.Styles.Add(new ImageStyle
                {
                    Image = $"file:///{dto.IconPath!.Replace("\\", "/")}",
                    SymbolScale = dto.Scale * scaleFactor, 
                    Opacity = 1,
                    Enabled = true
                });
            }
        }

        _markerLayer.DataHasChanged();
        Map.Refresh();
    }
}