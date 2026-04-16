

## 1. Real-World Context: The Algerian Tutoring Market 
In Algeria, private tutoring schools are a critical part of the educational ecosystem, hosting hundreds of students daily in intensive from 1 hour to 3-hour blocks. 
Historically, these institutions relied on manual bookkeeping, leading to massive inefficiencies.

This project was designed to automate three critical "Pain Points":

- **Dynamic Scheduling**: Managing limited classroom space against a high volume of subjects and levels.

- **Financial Tracking**: Moving away from paper receipts to a digital ledger that tracks student debt in real-time.

- **Automated Payroll**: Eliminating manual errors in calculating teacher commissions, which often vary by contract and student count.
## 2. Technology Stack & Decision Logic

The choice of technologies was driven by the specific infrastructure and operational habits of tutoring centers in Algeria, prioritizing stability over "trendy" cloud-first approaches.

### **Frontend: WPF (Windows Presentation Foundation)**
Instead of a web application, I developed a native desktop application using **WPF**. 
- **Rich UI/UX:** WPF’s powerful data-binding and XAML styling allowed for a professional, responsive interface that handles high-density data better than older frameworks.
- **Native Performance:** It provides seamless interaction with local hardware (such as thermal printers for student receipts) and the local file system for automatic backups.

### **Backend & Logic: C# / .NET**
I utilized the **.NET ecosystem** for its robustness in enterprise-level desktop software.
- **Business Logic:** C# allowed for the implementation of complex "Teacher Commission" logic and dynamic scheduling rules.
- **Type Safety:** Using a compiled language ensured that financial calculations for student debt and payments remained accurate and bug-free.

### **Database: Local SQL Server (Offline-First Strategy)**
The decision to use a **Local SQL Server** rather than a Cloud-based database was a strategic choice based on the Algerian local environment:

- **Connectivity Constraints:** Many tutoring centers do not have dedicated high-speed internet. Administrative staff often rely on **mobile hotspots (phone tethering)**, which can be unstable and expensive.
- **Operational Continuity:** A cloud-based system would experience significant latency or total downtime during peak hours if the connection dropped. 
- **Data Persistence:** By hosting the database locally, the school remains **100% functional offline**. This ensures that the system is always available to record attendance and payments, even in areas with poor network coverage.
- ### **Key Table Definitions**
## 3. System Design & Data Architecture

A primary goal of this project was to transform a chaotic, manual "pen-and-paper" workflow into a structured relational system. The design focuses on data integrity and real-time operational answers.

### **Entity Relationship Diagram (ERD)**
The database was architected to handle complex dependencies between students, their financial obligations, and classroom availability.

![Database Relation Diagram](./images/your-database-diagram.png) 
*Note: This diagram illustrates the relational mapping used to maintain ACID compliance across all school transactions.*

### **Data Intelligence: Solving Operational Challenges**

Rather than treating the database as simple storage, I designed the schema to answer critical "Business Intelligence" questions required during the high-pressure 1:30–3:00 PM student rush.

#### **Q: "Is the student cleared to enter the session?"**
In a manual system, tracking debt is slow. Here, the system performs a real-time calculation:
- **Logic:** The system queries the `Debt` table and aggregates all linked `Payment` records for that student.
- **Outcome:** If `RequiredFee - PaidAmount > 0`, the UI immediately flags the student in **Red**, ensuring the administration collects revenue before the lesson begins.

#### **Q: "How do we calculate teacher payroll accurately?"**
Teacher commissions in Algeria are often percentage-based (e.g., 40/60 or 50/50).
- **Logic:** The system joins the `Sessions`, `Attendance`, and `Teachers` tables. It calculates the total revenue from students present and applies the specific `CommissionRate` stored in the teacher's profile.
- **Outcome:** What used to take 2 hours of manual ledger work is now a one-click automated report.

#### **Q: "Is Room 01 available for Physics at 2:00 PM?"**
To prevent room overlapping, the system implements a **Validation Layer**:
- **Logic:** Before a new schedule is saved, the system runs a conflict-check query to ensure no other `Session` shares the same `RoomID` at the same `StartTime` and `EndTime`.
- **Outcome:** Physical resource conflicts are eliminated at the database level.

To manage the high volume of transactions, the database is structured around several core entities:

| Table Name | Purpose | Key Responsibility |
| :--- | :--- | :--- |
| **`Students`** | Identity Management | Stores unique identifiers, contact info, and academic levels for all enrolled pupils. |
| **`Sessions`** | Event Tracking | The "heart" of the system; links a **Teacher**, a **Subject**, and a **Room** to a specific time slot. |
| **`Attendance`** | Real-time Logging | Tracks which students were physically present in which session to trigger payment calculations. |
| **`Debts / Payments`** | Financial Ledger | Manages the "Account Balance" logic. Every registration creates a `Debt`, and every receipt creates a `Payment`. |
| **`TeacherCommissions`** | Revenue Split | Stores the specific percentage (e.g., 0.4 for 40%) used to calculate payroll based on attendance revenue. |
| **`Rooms`** | Resource Allocation | Defines physical classroom capacity to ensure the school doesn't over-enroll a session. |
