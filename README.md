# CapstoneCC (CamziesCollection)

This repository contains our project for Capstone 1 and 2, which is a requirement for our graduation. The project showcases our skills and knowledge in software development, web technologies, and teamwork.

## About the Project

CapstoneCC is a web application designed to demonstrate our ability to build a functional and user-friendly system. It includes features such as user management, social media integration, and more. The project was developed in collaboration with our client, **Camzies Collection**, to address their specific needs and requirements.

## Team Members

We are a group of three members who collaborated on this project:

- **Elmar Panganiban**
- **Andrea Sistorias**
- **Jairic Pinlac**

## Purpose

This project serves as a culmination of our learning and efforts throughout our academic journey. It is a testament to our dedication and hard work as we prepare for graduation. Additionally, it aims to provide a valuable solution to our client, **Camzies Collection**, by enhancing their business operations through technology.

## Technologies Used

- ASP.NET Core Razor Pages
- Entity Framework Core
- HTML, CSS, JavaScript
- SQL Server

## Features

- User Management System
- Social Media Integration
- Product Catalog and Inventory Management
- Order and Payment Processing
- Reporting and Analytics Dashboard

## How to Run the Project

1. Clone the repository.
2. Open the solution file `CapstoneCC.sln` in Visual Studio.
3. Restore NuGet packages.
4. Run the application using IIS Express or your preferred method.

## Acknowledgments

We would like to express our heartfelt gratitude to **Camzies Collection** for agreeing to be our client in this capstone project. Your trust and support have been invaluable in making this project a success. Thank you for giving us the opportunity to work on a real-world application and for providing us with insights and feedback throughout the development process.

## Contact

If you have any questions or feedback, feel free to reach out to us.

## Local Setup Guide

For the best experience, we recommend using **Visual Studio 2022**.

1. **Install Visual Studio 2022** (Community Edition is sufficient).
2. Ensure the following workloads are installed:
   - ASP.NET and web development
   - .NET desktop development
   - Data storage and processing (for SQL Server support)
3. **Clone the repository**:
   ```sh
   git clone https://github.com/yourusername/CapstoneCC.git
   ```
4. **Open the solution**: Double-click `CapstoneCC.sln` or open it from Visual Studio 2022.
5. **Restore NuGet packages**: Visual Studio will prompt you or restore automatically.
6. **Set up the database**:
   - Make sure SQL Server is running locally.
   - Update the connection string in `appsettings.json` if needed.
   - Apply migrations using the Package Manager Console:
     ```sh
     Update-Database
     ```
7. **Run the application**: Press F5 or click "Start" to launch with debugging.