# GradeGenie.Web (Next.js frontend)

Minimal Next.js app shell that talks to the GradeGenie API.

Prerequisites
- Node.js 18+ and npm
- The backend running locally (see repository root)

Configuration
1. Copy `.env.example` to `.env.local` and update the API base URL if needed:

```env
NEXT_PUBLIC_API_BASE_URL=https://localhost:5001
```

Running locally

```bash
npm install
npm run dev
```

Notes
- The UI expects an authenticated user: paste a JWT into the "JWT access token" field to use the protected API routes.
- Use the displayed Student ID to interact with the API (or load an existing student record).
- The project was scaffolded as a simple UI shell; further features (course editing, UX polish) can be added.
