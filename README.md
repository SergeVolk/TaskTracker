# About
TaskTracker is a task management tool.
It provides such functionality as creating tasks, assigning to a responsible person, scheduling for a certain time period, displaying according to selected filters, etc.

This repository contains sources of the following parts of the TaskTracker system:
- TaskTracker core. It contains the domain model, storage, other basic code.
- Desktop WPF client. It is a "fat client". The storage is built in the application.
- WCF Service and a thin desktop WPF client. The service is hosted in a standalone console app (server) and provides access to the storage.
- Tools for QA and management of the storage.