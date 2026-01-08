# GitHub Issues Mirror
![License](https://img.shields.io/badge/License-MIT-blue)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

A simple .NET service to sync GitHub issues and comments with a local database.

## Features

* **Mirror**: Fetch issues and comments from a repository to a local DB.
* **Webhooks**: Listen for GitHub events to update local data.
* **Two-way Sync**: Push local changes back to GitHub.
* **CRUD**: Create, update, and delete issues/comments.

## Configuration

Create a `.env` file in the root directory:

```env
GITHUB_TOKEN=your_token
GH_USERNAME=owner_name
GH_REPOSITORY=repo_name
DB_USERNAME=db_user
DB_PASSWORD=db_password

```

## API Endpoints

### GitHub to Local

* `GET /github/mirror` - Full sync from GitHub to DB.
* `POST /github/webhook` - Handles incoming GitHub events.

### Local to GitHub

* `POST /github/create/{number}` - Create local issue on GitHub.
* `POST /github/sync/{id}` - Update GitHub issue from local data.
* `POST /github/create/{issueNumber}/comments/{commentId}` - Post local comment to GitHub.
* `POST /github/sync/comment/{id}` - Update GitHub comment.
* `DELETE /github/delete/comments/{id}` - Delete comment from both GitHub and DB.

## Requirements

* .NET 10 SDK
* Entity Framework Core
* Newtonsoft.Json
* DotNetEnv

## Setup

### Docker
1. Configure the database in `IssuesContext`.
2. Create a `.env` file with your credentials.
3. Run:
   ```bash
   docker compose up --build -d

---
<img src="https://media1.tenor.com/m/jeQ1rV_L_tUAAAAd/yuuri-girlslastrour.gif" width="1000" height="300" alt="Demo">
