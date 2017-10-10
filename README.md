# About
TaskTracker is a tool for task management.
It provides such functionality as creating tasks, assigning them to a responsible person, scheduling them for a certain time period, displaying them according to selected filters, etc.

This repository contains sources of the following parts of the TaskTracker system:
- TaskTracker core. It contains the domain model, storage, other basic code.
- Desktop WPF client. It is a "fat client". The storage is built in the application.
- WCF Service and a thin desktop WPF client. The service is hosted in a standalone console app (server) and provides access to the storage.
- Tools for QA and management of the storage.