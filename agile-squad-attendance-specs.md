# Agile Squad Attendance Management Platform

## 🧩 Project Title
**Agile Squad Attendance Management Platform**

---

## 🏗️ Tech Stack
- **Backend:** Laravel 11 (API-only)
- **Frontend:** Vue 3 (Options API SPA)
- **Database:** MySQL or PostgreSQL
- **Authentication:** Laravel Sanctum (SSO + Email-based login)
- **Storage:** S3-compatible storage for attachments
- **Notifications:** Laravel Notifications (Email, Slack, MS Teams)
- **Background Jobs:** Laravel Queues (Redis or Database driver)
- **Integrations:** Google Calendar, Outlook, Slack/MS Teams, Jira

---

## 🚀 Core Features

### 1. Team Self-Onboarding
- **Authentication Options:** SSO (Google/Microsoft) or email/password.  
- **Auto Import from HR:** Option to sync user and squad data from HR API.  
- **Manual Squad Creation:** Admin can manually create squads if not imported.  
- **Default Roles:** `Admin`, `Squad Lead`, `Member`, `Viewer`.  

### 2. Squad Setup
- **Create and Manage Squads**
  - Fields: Name, Description, Linked Projects/Sprints.
  - Assign Squad Lead and Members.
- **Configuration Options:**
  - Time zone, Workdays, Sprint duration.
  - Link to project management tools (e.g., Jira board ID).

### 3. Check-In / Check-Out
- **Web & Mobile Friendly:** Quick buttons for check-in and check-out.
- **Location Tracking:** Optional geolocation or IP capture.
- **Auto-Checkout Reminder:** Automatic reminder if not checked out by day-end.

### 4. Sprint-Based Attendance Tracking
- **Sprint Context:** Attendance is tied to specific sprints rather than just calendar days.
- **Event Tags:** Tag sessions (Standup, Retro, Sprint Planning, Demo).
- **Status Flags:** Partial Day, Remote, Leave.

### 5. Leave Management
- **Leave Requests:** Submit with reason and optional documents.  
- **Approval Workflow:** Request → Squad Lead → Admin.  
- **Leave Types:** Vacation, Sick, Public Holiday, Training.  

### 6. Work Mode Status
- **Modes:** Remote, Office, Client Site, Out of Office (OOO).  
- **Real-Time Presence Board:** Display each member’s current status.
- **AI Summary (Optional Future Enhancement):** Summarize availability and absences by squad.

### 7. Attendance Rules & Compliance
- **Custom Rules per Squad:** Minimum working hours, late arrivals, early check-outs.
- **Compliance Alerts:** Notify leads/admins of anomalies.
- **Scoring:** Weekly compliance score auto-generated per squad.

### 8. Integrations
- **Calendar:** Sync with Google/Outlook for holidays and sprint events.
- **Communication:** Slack/MS Teams bot for check-ins, reminders, and reports.
- **Project Management:** Jira integration for linking attendance with sprint tasks.

### 9. Reports & Insights
- **Dashboards:** Daily, Weekly, and Sprint attendance summaries.
- **Filters:** By Squad, Member, Project, or Date Range.
- **Exports:** CSV and PDF export.
- **Automated Reports:** Scheduled HR emails per sprint.

### 10. Role-Based Access

| Role | Permissions |
|------|--------------|
| **Admin** | Manage squads, users, rules, and org-wide reports |
| **Squad Lead** | Approve leaves, view squad analytics |
| **Member** | Check-in/out, manage attendance, request leave |
| **Viewer** | Read-only access to dashboards |

### 11. Notifications
- **Types:** Daily standup reminders, Missed check-in alerts, Leave approval notifications, Sprint summary reports.  
- **Channels:** Email, Slack, or MS Teams.

### 12. Audit Trail
- **Full Logging:** Record all edits (check-ins, leave requests, approvals) with timestamps.
- **Immutable History:** Change history stored for HR compliance and review.

---

## 🧠 Additional Modules (Optional Enhancements)
- AI-based anomaly detection for attendance irregularities.  
- Predictive leave trend analysis.  
- HR dashboard with attendance insights across squads.

---

## 🎨 Frontend (Vue.js SPA)
- **Routing:** Vue Router with role-based route guards.
- **State Management:** Pinia or Vuex (for auth and squad data).
- **UI Framework:** Tailwind CSS + Headless UI.
- **Core Pages:**
  - Login / SSO Callback  
  - Dashboard  
  - Squad Management  
  - Attendance Board  
  - Leave Requests  
  - Reports & Analytics  
  - Settings (Integrations, Rules, Roles)

---

## ⚙️ Backend (Laravel API)
- **Modules:**
  - Authentication (SSO + Sanctum)
  - Squad & Member Management
  - Attendance & Sprint Management
  - Leave Management
  - Compliance Rules
  - Notifications
  - Audit Logging
  - Reports API
  - Integration Services (Google, Slack, Jira)
