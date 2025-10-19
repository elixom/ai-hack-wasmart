/**
 * SignalR Notifications Client
 * Handles real-time notifications for GreenSync application
 */

class NotificationManager {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.retryCount = 0;
        this.maxRetries = 5;
        this.retryDelay = 5000; // 5 seconds
        
        this.init();
    }

    /**
     * Initialize SignalR connection
     */
    async init() {
        try {
            // Create connection
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/notificationHub")
                .withAutomaticReconnect([0, 2000, 10000, 30000])
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // Set up event handlers
            this.setupEventHandlers();

            // Start connection
            await this.connect();
            
        } catch (error) {
            console.error('Failed to initialize SignalR connection:', error);
            this.scheduleReconnect();
        }
    }

    /**
     * Setup SignalR event handlers
     */
    setupEventHandlers() {
        // Connection events
        this.connection.onreconnecting((error) => {
            console.log('SignalR reconnecting...', error);
            this.isConnected = false;
            this.showConnectionStatus('Reconnecting...', 'warning');
        });

        this.connection.onreconnected((connectionId) => {
            console.log('SignalR reconnected:', connectionId);
            this.isConnected = true;
            this.retryCount = 0;
            this.showConnectionStatus('Connected', 'success');
            setTimeout(() => this.hideConnectionStatus(), 3000);
        });

        this.connection.onclose((error) => {
            console.log('SignalR connection closed:', error);
            this.isConnected = false;
            this.showConnectionStatus('Connection lost', 'danger');
            this.scheduleReconnect();
        });

        // Admin notification for regular users
        this.connection.on("ReceiveAdminNotification", (notification) => {
            console.log('Received admin notification:', notification);
            this.showNotification(notification);
            this.playNotificationSound();
        });

        // New report notification for admins
        this.connection.on("ReceiveNewReportNotification", (notification) => {
            console.log('Received new report notification:', notification);
            this.showAdminReportNotification(notification);
            this.playNotificationSound();
            this.updateReportCounter();
        });

        // Report status update for users
        this.connection.on("ReceiveReportStatusUpdate", (notification) => {
            console.log('Received report status update:', notification);
            this.showNotification(notification);
            this.playNotificationSound();
        });

        // General notifications
        this.connection.on("ReceiveNotification", (notification) => {
            console.log('Received general notification:', notification);
            this.showNotification(notification);
        });
        // General ReceiveReportCount
        this.connection.on("ReceiveReportCount", (notification) => {
            console.log('Received ReceiveReportCount notification:', notification);
            this.updateReportCounter(notification.reportCount);
        }); 
    }

    /**
     * Connect to SignalR hub
     */
    async connect() {
        try {
            await this.connection.start();
            this.isConnected = true;
            this.retryCount = 0;
            console.log('SignalR connected successfully');
            this.showConnectionStatus('Connected', 'success');
            setTimeout(() => this.hideConnectionStatus(), 3000);
        } catch (error) {
            console.error('SignalR connection failed:', error);
            this.isConnected = false;
            this.scheduleReconnect();
        }
    }

    /**
     * Schedule reconnection attempt
     */
    scheduleReconnect() {
        if (this.retryCount < this.maxRetries) {
            this.retryCount++;
            console.log(`Scheduling reconnection attempt ${this.retryCount}/${this.maxRetries} in ${this.retryDelay}ms`);
            
            setTimeout(() => {
                this.connect();
            }, this.retryDelay * this.retryCount);
        } else {
            console.error('Max reconnection attempts reached');
            this.showConnectionStatus('Connection failed - Please refresh page', 'danger');
        }
    }

    /**
     * Show general notification
     */
    showNotification(notification) {
        // Create toast notification
        const toast = this.createToast(notification);
        this.showToast(toast);
        
        // Store notification for later viewing
        this.storeNotification(notification);
    }

    /**
     * Show admin report notification
     */
    showAdminReportNotification(notification) {
        // Show as toast first
        const toast = this.createReportToast(notification);
        this.showToast(toast);
        
        // Add to admin dashboard if on admin page
        if (window.location.pathname.includes('/Admin/')) {
            this.addToAdminDashboard(notification);
        }
        
        // Store notification
        this.storeNotification(notification);
    }

    /**
     * Create toast notification element
     */
    createToast(notification) {
        const toastId = `toast-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        const typeClass = this.getBootstrapClass(notification.type);
        const iconClass = this.getIconClass(notification.type);
        const timestamp = this.formatTimestamp(notification.timestamp);

        return `
            <div id="${toastId}" class="toast align-items-center text-bg-${typeClass} border-0" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="10000">
                <div class="d-flex">
                    <div class="toast-body">
                        <div class="d-flex align-items-start">
                            <i class="bi ${iconClass} me-2 mt-1"></i>
                            <div class="flex-grow-1">
                                <h6 class="mb-1 fw-bold">${this.escapeHtml(notification.title)}</h6>
                                <p class="mb-1">${this.escapeHtml(notification.message)}</p>
                                <small class="opacity-75">${timestamp}${notification.fromAdmin ? ' • From Admin' : ''}</small>
                            </div>
                        </div>
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;
    }

    /**
     * Create report-specific toast
     */
    createReportToast(notification) {
        const toastId = `toast-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        const timestamp = this.formatTimestamp(notification.timestamp);

        return `
            <div id="${toastId}" class="toast align-items-center text-bg-info border-0" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="15000">
                <div class="d-flex">
                    <div class="toast-body">
                        <div class="d-flex align-items-start">
                            <i class="bi bi-exclamation-circle me-2 mt-1"></i>
                            <div class="flex-grow-1">
                                <h6 class="mb-1 fw-bold">${this.escapeHtml(notification.title)}</h6>
                                <p class="mb-1">${this.escapeHtml(notification.message)}</p>
                                <div class="small mt-2">
                                    <strong>Priority:</strong> <span class="badge bg-${this.getPriorityClass(notification.priority)}">${notification.priority}</span><br>
                                    <strong>Type:</strong> ${notification.wasteType}<br>
                                    <strong>Volume:</strong> ${notification.volume} m³
                                </div>
                                <small class="opacity-75 d-block mt-1">${timestamp}</small>
                            </div>
                        </div>
                        <div class="mt-2">
                            <a href="/Admin/Home/Index" class="btn btn-sm btn-light">View Dashboard</a>
                        </div>
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;
    }

    /**
     * Show toast notification
     */
    showToast(toastHtml) {
        // Ensure toast container exists
        this.ensureToastContainer();
        
        // Add toast to container
        const container = document.getElementById('toast-container');
        container.insertAdjacentHTML('beforeend', toastHtml);
        
        // Initialize and show toast
        const toastElement = container.lastElementChild;
        const toast = new bootstrap.Toast(toastElement);
        toast.show();
        
        // Clean up after toast is hidden
        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    }

    /**
     * Ensure toast container exists
     */
    ensureToastContainer() {
        let container = document.getElementById('toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'toast-container position-fixed top-0 end-0 p-3';
            container.style.zIndex = '9999';
            document.body.appendChild(container);
        }
    }

    /**
     * Show connection status
     */
    showConnectionStatus(message, type) {
        // Remove existing status
        this.hideConnectionStatus();
        
        const statusHtml = `
            <div id="connection-status" class="alert alert-${type} alert-dismissible fade show position-fixed" role="alert" 
                 style="top: 10px; right: 10px; z-index: 10000; min-width: 250px;">
                <i class="bi bi-wifi me-2"></i>${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        
        document.body.insertAdjacentHTML('beforeend', statusHtml);
    }

    /**
     * Hide connection status
     */
    hideConnectionStatus() {
        const status = document.getElementById('connection-status');
        if (status) {
            status.remove();
        }
    }

    /**
     * Play notification sound
     */
    playNotificationSound() {
        // Create audio element and play notification sound
        try {
            const audio = new Audio('data:audio/wav;base64,UklGRnoGAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQoGAACBhYqFbF1fdJivrJBhNjVgodDbq2EcBj+a2/LDciUFLIHO8tiJNwgZaLvt559NEAxQp+PwtmMcBjiR1/LMeSwFJHfH8N2QQAoUXrTp66hVFApGn+DyvmUkBjaHzvHZjToHGGGw6+OZSA0PUqPh77BdGwU+ltryxnkpBSl+zPDajTkIGGS57OOaSwwOUKXj8LNfHgU7k9n0yHkpBS1+y/HYjTkHGGu//Ddjtyljx7YmnJTRjhHBSXREGGhfaIVqhG2JqaC9bHOJmo6YjY6VkJCPjoqLBDPHXtY7mFjLR5z5K5ZA1s38uKq3oj6LDPNvwsrjz8f9KKZezt/qlz7Qo8B+rKqIgqWj9JjC7+H1KJJb2urq0MbO/iaZY7/z9KXJ7yGOUdLu3uFDqDUFHmY=');
            audio.volume = 0.3;
            audio.play();
        } catch (error) {
            console.log('Could not play notification sound:', error);
        }
    }

    /**
     * Update report counter for admin
     */
    updateReportCounter(newCount = null) {
        const counter = document.querySelector('[data-field="open-reports-counter"]');
        if (counter) {
            const currentCount = parseInt(counter.textContent) || 0;
            if (newCount !== null) {
                currentCount = parseInt(newCount) || 0;
            } else {
                counter.textContent = currentCount + 1;
            }
            counter.style.display = 'inline';
        }
    }

    /**
     * Store notification in localStorage for later viewing
     */
    storeNotification(notification) {
        try {
            const stored = JSON.parse(localStorage.getItem('greensync_notifications') || '[]');
            stored.unshift({
                ...notification,
                id: Date.now(),
                read: false
            });
            
            // Keep only last 50 notifications
            if (stored.length > 50) {
                stored.splice(50);
            }
            
            localStorage.setItem('greensync_notifications', JSON.stringify(stored));
        } catch (error) {
            console.error('Failed to store notification:', error);
        }
    }

    /**
     * Get Bootstrap alert class for notification type
     */
    getBootstrapClass(type) {
        const mapping = {
            'success': 'success',
            'info': 'info',
            'warning': 'warning',
            'danger': 'danger'
        };
        return mapping[type] || 'info';
    }

    /**
     * Get icon class for notification type
     */
    getIconClass(type) {
        const mapping = {
            'success': 'bi-check-circle-fill',
            'info': 'bi-info-circle-fill',
            'warning': 'bi-exclamation-triangle-fill',
            'danger': 'bi-x-circle-fill'
        };
        return mapping[type] || 'bi-info-circle-fill';
    }

    /**
     * Get priority badge class
     */
    getPriorityClass(priority) {
        const mapping = {
            'Low': 'secondary',
            'Medium': 'primary',
            'High': 'warning',
            'Critical': 'danger'
        };
        return mapping[priority] || 'secondary';
    }

    /**
     * Format timestamp for display
     */
    formatTimestamp(timestamp) {
        try {
            const date = new Date(timestamp);
            const now = new Date();
            const diffMs = now - date;
            const diffMins = Math.floor(diffMs / 60000);
            
            if (diffMins < 1) return 'Just now';
            if (diffMins < 60) return `${diffMins}m ago`;
            
            const diffHours = Math.floor(diffMins / 60);
            if (diffHours < 24) return `${diffHours}h ago`;
            
            return date.toLocaleDateString();
        } catch (error) {
            return 'Unknown';
        }
    }

    /**
     * Escape HTML to prevent XSS
     */
    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    /**
     * Add notification to admin dashboard
     */
    addToAdminDashboard(notification) {
        const dashboard = document.getElementById('admin-notifications');
        if (dashboard) {
            const notificationHtml = `
                <div class="alert alert-info alert-dismissible fade show">
                    <div class="d-flex align-items-start">
                        <i class="bi bi-exclamation-circle me-2 mt-1"></i>
                        <div class="flex-grow-1">
                            <h6 class="mb-1">${this.escapeHtml(notification.Title)}</h6>
                            <p class="mb-1">${this.escapeHtml(notification.Message)}</p>
                            <small class="text-muted">${this.formatTimestamp(notification.Timestamp)}</small>
                        </div>
                    </div>
                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                </div>
            `;
            dashboard.insertAdjacentHTML('afterbegin', notificationHtml);
        }
    }

    /**
     * Get connection status
     */
    getConnectionStatus() {
        return {
            isConnected: this.isConnected,
            connectionId: this.connection?.connectionId,
            retryCount: this.retryCount
        };
    }
}

// Initialize notification manager when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    if (typeof signalR !== 'undefined') {
        window.notificationManager = new NotificationManager();
        
        // Expose for debugging
        window.getNotificationStatus = () => window.notificationManager.getConnectionStatus();
    } else {
        console.warn('SignalR library not loaded - notifications will not work');
    }
});
