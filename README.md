# JobConnect – Job Portal Web Application

JobConnect is a web-based job portal built with **ASP.NET Core MVC**, designed to connect **employers** and **job seekers** efficiently. The platform allows job seekers to find suitable job opportunities, while employers can post job offers, manage applications, and find the ideal candidate.

---

## Features

### For Job Seekers:
- Browse and search for jobs with filtering options
- Apply for available jobs
- View application status (Pending / Accepted / Rejected)
- Edit profile and upload CV/resume
- Delete profile and applications

### For Employers:
- Post jobs (up to 3 active jobs for Basic plan; unlimited for Paid plan)
- Edit or delete posted jobs
- View applicants for each job and access their profile
- Approve or reject applicants
- Contact applicants via email directly through the platform
- Profile management for company details and contact information

### General Features:
- Role-based authentication (Job Seeker / Employer)
- Email verification during registration via token
- Forgot password functionality with email-based password reset
- Rate limiting to prevent brute-force attacks
- Stripe integration for Paid plan subscriptions (1€/month)
- SQL Server database with Entity Framework Core
- Fully responsive and user-friendly interface using HTML, CSS, and JavaScript

---

## Technologies Used

- **Backend:** ASP.NET Core MVC
- **Frontend:** HTML, CSS, JavaScript
- **Database:** SQL Server
- **Authentication & Security:** Email verification, Rate Limiting
- **Payments:** Stripe API
- **Email:** SMTP service for notifications and password reset
- **ORM:** Entity Framework Core
- **Version Control:** Git / GitHub

---


