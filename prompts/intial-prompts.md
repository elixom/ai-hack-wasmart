**Project Title:** AI-Powered Smart Waste & Resource Management Platform

**Technology & Constraints (MANDATORY):**
1.  **Architecture:** You MUST use the **C# MVC (.NET) Framework**.
2.  **UI Framework:** Blazor IS STRICTLY FORBIDDEN. All views must be generated using Razor Views (.cshtml) with standard HTML, CSS (Bootstrap/Tailwind preferred for aesthetics), and JavaScript.
3.  **Rules Compliance:** The generated code MUST fully comply with all established "cline rules," including the single-file mandate for initial prototyping if applicable, otherwise focusing on a clean, scalable C# MVC structure.

**Core Functional Requirements:**

The application requires three distinct user roles (managed via simple session/mock authentication): Resident/Commercial User, and Municipal Administrator.

**1. Data Model:**
* **Report:** ID, Location (Mock Geo-coordinates), Status (Reported, Assigned, Collected), Image URL (Placeholder string), Timestamp.
* **EcoCredit:** UserID, CurrentBalance, TransactionHistory.
* **Route:** ID, OptimizedPath (Array/List of Mock Geo-coordinates), Duration, Fuel Savings Metric (Mock calculation).

**2. User & Commercial Portal (Role: User):**
* **Real-Time Reporting:** A view/form allowing users to submit a waste report. The form must accept a description and a mock geolocation coordinate/address string.
* **Proactive Alerts Simulation:** A section to display mock notifications related to scheduled collection times or pickup confirmations.
* **Eco-Credit View:** A dedicated page to display the user’s current Eco-Credit balance and a history of transactions/rewards earned.

**3. Administrator Command Center (Role: Admin):**
* **Fleet Management Dashboard (Home View):** A high-level overview displaying the total number of open reports, total trucks deployed (mock count), and a visual representation (simple table or chart) of the predicted waste hotspots.
* **Dynamic Route Optimization Simulation:** Create an MVC Controller Action (`RouteController.Optimize`) that, when triggered (e.g., via a button on the Admin Dashboard), simulates the AI's function. This simulation should:
    * Fetch a mock list of uncollected reports/pickups.
    * Apply a simple C# algorithm (e.g., distance sorting or grouping by quadrant) to generate a "simulated optimal route."
    * Store the resulting Route object. The view should display the simulated operational efficiency gains (e.g., "Simulated 18% Fuel Reduction").

**4. Data Persistence:**
* Use in-memory data structures (lists, dictionaries) for all persistence to keep the project self-contained and runnable without external database setup. Provide clear C# classes for the required models (Report, EcoCredit, Route).

**Objective:** Deliver a secure, well-structured C# MVC application that prioritizes the simulation of the platform's core *AI-Powered Predictive Routing* and the *Eco-Credit Incentive Ledger* features.