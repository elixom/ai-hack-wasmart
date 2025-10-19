// Azure Maps initialization for Waste Report Locations
(function() {
    'use strict';

    // Wait for DOM to be ready
    document.addEventListener('DOMContentLoaded', function() {
        initializeMap();
    });

    function initializeMap() {
        // Check if config is available
        if (!window.azureMapsConfig || !window.azureMapsConfig.subscriptionKey) {
            console.error('Azure Maps configuration not found');
            showMapError('Map configuration is missing. Please add your Azure Maps subscription key to appsettings.json');
            return;
        }

        const config = window.azureMapsConfig;

        // Default center (can be customized based on user location or reports)
        const defaultCenter = [-122.3321, 47.6062]; // Seattle coordinates as default
        const defaultZoom = 11;

        try {
            // Initialize the map with subscription key authentication
            const map = new atlas.Map('wasteReportsMap', {
                center: defaultCenter,
                zoom: defaultZoom,
                language: 'en-US',
                authOptions: {
                    authType: 'subscriptionKey',
                    subscriptionKey: config.subscriptionKey
                }
            });

            // Wait for map to be ready
            map.events.add('ready', function() {
                // Add zoom control
                map.controls.add(new atlas.control.ZoomControl(), {
                    position: 'top-right'
                });

                // Add compass control
                map.controls.add(new atlas.control.CompassControl(), {
                    position: 'top-right'
                });

                // Add pitch control
                map.controls.add(new atlas.control.PitchControl(), {
                    position: 'top-right'
                });

                // Add style control
                map.controls.add(new atlas.control.StyleControl({
                    mapStyles: 'all'
                }), {
                    position: 'top-left'
                });

                // Create data source for markers
                const dataSource = new atlas.source.DataSource();
                map.sources.add(dataSource);

                // Add report markers to the map
                if (config.reports && config.reports.length > 0) {
                    addReportMarkers(map, dataSource, config.reports);
                } else {
                    // Show message when no reports
                    showNoReportsMessage();
                }
            });

            // Handle map errors
            map.events.add('error', function(error) {
                console.error('Map error:', error);
                showMapError('Error loading map. Please check your Azure Maps configuration.');
            });

        } catch (error) {
            console.error('Error initializing map:', error);
            showMapError('Failed to initialize map');
        }
    }

    function addReportMarkers(map, dataSource, reports) {
        const features = [];
        const bounds = new atlas.data.BoundingBox();

        reports.forEach(function(report) {
            // Only add markers for reports with valid coordinates
            if (report.latitude && report.longitude) {
                const coordinates = [report.longitude, report.latitude];

                // Create a point feature
                const feature = new atlas.data.Feature(new atlas.data.Point(coordinates), {
                    id: report.id,
                    location: report.location,
                    status: report.status,
                    wasteType: report.wasteType,
                    timestamp: report.timestamp
                });

                features.push(feature);

                // Expand bounds to include this point
                bounds.extend(coordinates);
            }
        });

        // Add features to data source
        dataSource.add(features);

        // Create symbol layer for markers
        const symbolLayer = new atlas.layer.SymbolLayer(dataSource, null, {
            iconOptions: {
                image: 'marker-red',
                anchor: 'bottom',
                size: 0.8
            },
            textOptions: {
                textField: ['get', 'wasteType'],
                offset: [0, -2],
                size: 12,
                color: '#ffffff',
                haloColor: '#000000',
                haloWidth: 2
            }
        });

        map.layers.add(symbolLayer);

        // Create popup
        const popup = new atlas.Popup({
            pixelOffset: [0, -18],
            closeButton: true
        });

        // Add click event to show popup
        map.events.add('click', symbolLayer, function(e) {
            if (e.shapes && e.shapes.length > 0) {
                const properties = e.shapes[0].getProperties();
                
                // Get status badge HTML
                const statusBadge = getStatusBadge(properties.status);
                
                // Create popup content
                const content = `
                    <div class="popup-content" style="padding: 10px; min-width: 200px;">
                        <h6 style="margin-bottom: 10px; color: #198754;">
                            <i class="bi bi-geo-alt-fill"></i> ${properties.location}
                        </h6>
                        <div style="margin-bottom: 5px;">
                            <strong>Status:</strong> ${statusBadge}
                        </div>
                        <div style="margin-bottom: 5px;">
                            <strong>Type:</strong> ${properties.wasteType}
                        </div>
                        <div style="margin-bottom: 10px;">
                            <strong>Date:</strong> ${properties.timestamp}
                        </div>
                        <a href="/Reports/Details/${properties.id}" class="btn btn-sm btn-outline-primary">
                            <i class="bi bi-eye"></i> View Details
                        </a>
                    </div>
                `;

                popup.setOptions({
                    content: content,
                    position: e.shapes[0].getCoordinates()
                });

                popup.open(map);
            }
        });

        // Change cursor on hover
        map.events.add('mousemove', symbolLayer, function() {
            map.getCanvasContainer().style.cursor = 'pointer';
        });

        map.events.add('mouseout', symbolLayer, function() {
            map.getCanvasContainer().style.cursor = 'grab';
        });

        // Set map view to show all markers
        if (features.length > 0) {
            map.setCamera({
                bounds: bounds,
                padding: 50
            });
        }
    }

    function getStatusBadge(status) {
        const badges = {
            'Reported': '<span class="badge bg-warning">Reported</span>',
            'Assigned': '<span class="badge bg-info">Assigned</span>',
            'InProgress': '<span class="badge bg-primary">In Progress</span>',
            'Collected': '<span class="badge bg-success">Collected</span>'
        };
        return badges[status] || `<span class="badge bg-secondary">${status}</span>`;
    }

    function showMapError(message) {
        const mapContainer = document.getElementById('wasteReportsMap');
        if (mapContainer) {
            mapContainer.innerHTML = `
                <div class="d-flex align-items-center justify-content-center h-100">
                    <div class="text-center p-4">
                        <i class="bi bi-exclamation-triangle text-warning" style="font-size: 3rem;"></i>
                        <p class="text-muted mt-3">${message}</p>
                        <small class="text-muted">Please contact support if this issue persists.</small>
                    </div>
                </div>
            `;
        }
    }

    function showNoReportsMessage() {
        const mapContainer = document.getElementById('wasteReportsMap');
        if (mapContainer) {
            // Keep the map but show an overlay message
            const overlay = document.createElement('div');
            overlay.className = 'position-absolute top-50 start-50 translate-middle text-center';
            overlay.style.zIndex = '1000';
            overlay.style.backgroundColor = 'rgba(255, 255, 255, 0.9)';
            overlay.style.padding = '20px';
            overlay.style.borderRadius = '8px';
            overlay.style.boxShadow = '0 2px 10px rgba(0,0,0,0.1)';
            overlay.innerHTML = `
                <i class="bi bi-pin-map text-muted" style="font-size: 3rem;"></i>
                <p class="text-muted mt-3 mb-0">No waste reports to display</p>
                <small class="text-muted">Submit your first report to see it on the map!</small>
            `;
            mapContainer.parentElement.style.position = 'relative';
            mapContainer.parentElement.appendChild(overlay);
        }
    }
})();
