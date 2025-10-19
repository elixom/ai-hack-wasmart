// GreenSync JavaScript Functions

// Initialize application when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeAnimations();
    initializeTooltips();
    initializeGeolocation();
    initializeFormValidation();
    initializeEcoCreditsUpdater();
});

// Initialize CSS animations for elements
function initializeAnimations() {
    // Add fade-in animation to cards
    const cards = document.querySelectorAll('.card');
    cards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
        card.classList.add('fade-in');
    });

    // Add slide-in animation to stats cards
    const statsCards = document.querySelectorAll('.col-md-3 .card');
    statsCards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.2}s`;
        card.classList.add('slide-in-left');
    });
}

// Initialize Bootstrap tooltips
function initializeTooltips() {
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
}

// Geolocation functions for waste reporting
function initializeGeolocation() {
    const locationBtn = document.getElementById('getCurrentLocation');
    if (locationBtn) {
        locationBtn.addEventListener('click', getCurrentLocation);
    }
}

function getCurrentLocation() {
    const latInput = document.getElementById('Latitude');
    const lngInput = document.getElementById('Longitude');
    const locationInput = document.getElementById('Location');

    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
            function(position) {
                latInput.value = position.coords.latitude.toFixed(6);
                lngInput.value = position.coords.longitude.toFixed(6);
                
                // Reverse geocoding (mock implementation)
                locationInput.value = `Current Location (${position.coords.latitude.toFixed(4)}, ${position.coords.longitude.toFixed(4)})`;
                
                showSuccessMessage('Location detected successfully!');
            },
            function(error) {
                console.error('Geolocation error:', error);
                showErrorMessage('Unable to detect your location. Please enter manually.');
            }
        );
    } else {
        showErrorMessage('Geolocation is not supported by your browser.');
    }
}

// Form validation enhancements
function initializeFormValidation() {
    const forms = document.querySelectorAll('.needs-validation');
    forms.forEach(form => {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });

    // Real-time validation for report form
    const reportForm = document.querySelector('form[action*="Reports"]');
    if (reportForm) {
        const locationInput = reportForm.querySelector('#Location');
        if (locationInput) {
            locationInput.addEventListener('blur', validateLocation);
        }
    }
}

function validateLocation() {
    const locationInput = document.getElementById('Location');
    const value = locationInput.value.trim();
    
    if (value.length < 3) {
        locationInput.classList.add('is-invalid');
        showFieldError('Location', 'Please provide a more detailed location');
    } else {
        locationInput.classList.remove('is-invalid');
        locationInput.classList.add('is-valid');
    }
}

// Eco-Credits balance updater
function initializeEcoCreditsUpdater() {
    updateEcoCreditsDisplay();
    setInterval(updateEcoCreditsDisplay, 30000); // Update every 30 seconds
}

async function updateEcoCreditsDisplay() {
    try {
        const response = await fetch('/EcoCredits/Balance');
        if (response.ok) {
            const data = await response.json();
            const balanceElements = document.querySelectorAll('.eco-credit-balance');
            balanceElements.forEach(element => {
                if (element.textContent !== data.balance.toString()) {
                    animateNumberChange(element, data.balance);
                }
            });
        }
    } catch (error) {
        console.error('Error updating eco-credits:', error);
    }
}

function animateNumberChange(element, newValue) {
    const currentValue = parseInt(element.textContent) || 0;
    const increment = (newValue - currentValue) / 20;
    let currentStep = 0;

    const animation = setInterval(() => {
        currentStep++;
        const displayValue = Math.round(currentValue + (increment * currentStep));
        element.textContent = displayValue;

        if (currentStep >= 20) {
            element.textContent = newValue;
            clearInterval(animation);
            // Add a subtle glow effect for new credits
            if (newValue > currentValue) {
                element.classList.add('text-success');
                setTimeout(() => element.classList.remove('text-success'), 2000);
            }
        }
    }, 50);
}

// Utility functions for user feedback
function showSuccessMessage(message) {
    showToast(message, 'success');
}

function showErrorMessage(message) {
    showToast(message, 'danger');
}

function showInfoMessage(message) {
    showToast(message, 'info');
}

function showToast(message, type = 'info') {
    // Create toast container if it doesn't exist
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
        toastContainer.style.zIndex = '1055';
        document.body.appendChild(toastContainer);
    }

    // Create toast element
    const toastEl = document.createElement('div');
    toastEl.className = `toast align-items-center text-bg-${type} border-0`;
    toastEl.setAttribute('role', 'alert');
    toastEl.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;

    toastContainer.appendChild(toastEl);

    // Initialize and show toast
    const toast = new bootstrap.Toast(toastEl, {
        autohide: true,
        delay: 5000
    });
    toast.show();

    // Remove toast element after it's hidden
    toastEl.addEventListener('hidden.bs.toast', () => {
        toastEl.remove();
    });
}

function showFieldError(fieldName, message) {
    const field = document.getElementById(fieldName);
    if (field) {
        let errorDiv = field.parentNode.querySelector('.invalid-feedback');
        if (!errorDiv) {
            errorDiv = document.createElement('div');
            errorDiv.className = 'invalid-feedback';
            field.parentNode.appendChild(errorDiv);
        }
        errorDiv.textContent = message;
    }
}

// Report status tracking
function trackReportStatus(reportId) {
    // Simulate status updates
    const statusElement = document.getElementById(`report-${reportId}-status`);
    if (statusElement) {
        setTimeout(() => {
            statusElement.textContent = 'Assigned';
            statusElement.className = 'badge bg-info';
            showInfoMessage('Your report has been assigned to a collection truck!');
        }, 5000);
    }
}

// Route optimization animation
function animateRouteOptimization() {
    const optimizeBtn = document.querySelector('.btn-optimize');
    if (optimizeBtn) {
        optimizeBtn.addEventListener('click', function() {
            this.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span> Optimizing Route...';
            this.disabled = true;
        });
    }
}

// Eco-Credits redemption confirmation
function confirmRedemption(optionName, cost) {
    return confirm(`Are you sure you want to redeem "${optionName}" for ${cost} Eco-Credits?`);
}

// Progressive enhancement for forms
function enhanceReportForm() {
    const reportForm = document.querySelector('#reportForm');
    if (reportForm) {
        // Add current location button
        const locationGroup = document.querySelector('#Location').closest('.mb-3');
        const locationBtn = document.createElement('button');
        locationBtn.type = 'button';
        locationBtn.className = 'btn btn-outline-success btn-sm mt-2';
        locationBtn.id = 'getCurrentLocation';
        locationBtn.innerHTML = '<i class="bi bi-geo-alt"></i> Use My Current Location';
        locationGroup.appendChild(locationBtn);
        
        locationBtn.addEventListener('click', getCurrentLocation);
    }
}

// Initialize route optimization dashboard
function initializeRouteOptimizationDashboard() {
    const optimizationResults = document.querySelectorAll('.optimization-result');
    optimizationResults.forEach(result => {
        // Add hover effects and animations
        result.addEventListener('mouseenter', function() {
            this.style.transform = 'scale(1.02)';
        });
        
        result.addEventListener('mouseleave', function() {
            this.style.transform = 'scale(1)';
        });
    });
}

// Environmental impact calculator
function calculateEnvironmentalImpact(ecoCredits) {
    return {
        co2Saved: Math.round(ecoCredits * 0.5 * 10) / 10, // kg CO2
        treesEquivalent: Math.round(ecoCredits * 0.02 * 10) / 10,
        waterSaved: Math.round(ecoCredits * 1.5), // liters
        wasteReports: Math.floor(ecoCredits / 10)
    };
}

// Update environmental impact display
function updateEnvironmentalImpact(ecoCredits) {
    const impact = calculateEnvironmentalImpact(ecoCredits);
    
    const co2Element = document.querySelector('.co2-saved');
    const treesElement = document.querySelector('.trees-equivalent');
    const waterElement = document.querySelector('.water-saved');
    const reportsElement = document.querySelector('.reports-submitted');
    
    if (co2Element) co2Element.textContent = impact.co2Saved;
    if (treesElement) treesElement.textContent = impact.treesEquivalent;
    if (waterElement) waterElement.textContent = impact.waterSaved;
    if (reportsElement) reportsElement.textContent = impact.wasteReports;
}

// Auto-dismiss alerts after 5 seconds
setTimeout(function() {
    const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(alert => {
        const bsAlert = new bootstrap.Alert(alert);
        setTimeout(() => {
            try { bsAlert.close(); } catch (e) { console.log('Alert already closed'); }
        }, 5000);
    });
}, 1000);

// Add loading states to forms
function addLoadingStates() {
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function() {
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                const originalText = submitBtn.innerHTML;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span> Processing...';
                submitBtn.disabled = true;
                
                // Re-enable after 10 seconds as failsafe
                setTimeout(() => {
                    submitBtn.innerHTML = originalText;
                    submitBtn.disabled = false;
                }, 10000);
            }
        });
    });
}

// Initialize all enhancements
document.addEventListener('DOMContentLoaded', function() {
    enhanceReportForm();
    animateRouteOptimization();
    initializeRouteOptimizationDashboard();
    addLoadingStates();
    
    // Update environmental impact if eco-credits are displayed
    const ecoCreditsElement = document.querySelector('.eco-credit-balance');
    if (ecoCreditsElement) {
        const credits = parseFloat(ecoCreditsElement.textContent) || 0;
        updateEnvironmentalImpact(credits);
    }
});
