"use client";

import { FormEvent, useState } from "react";

type Student = { id: string; fullName: string };
type Course = { id: string; code: string; title: string; creditUnits: number; grade: string; gradePoint: number };
type Semester = { id: string; name: string; academicYear: number; gpa: number; totalCreditUnits: number; courses: Course[] };
type Cgpa = { studentId: string; fullName: string; cgpa: number; semesters: Semester[] };
const apiBase = process.env.NEXT_PUBLIC_API_BASE_URL ?? "https://localhost:5001";

async function request<T>(path: string, token: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBase}${path}`, { ...init, headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}`, ...init?.headers } });
  if (!response.ok) throw new Error(response.status === 404 ? "Record not found or not owned by this account." : `Request failed (${response.status}).`);
  return response.json() as Promise<T>;
}

export default function Home() {
  const [token, setToken] = useState(""); const [studentId, setStudentId] = useState(""); const [name, setName] = useState("");
  const [data, setData] = useState<Cgpa | null>(null); const [semesterName, setSemesterName] = useState(""); const [year, setYear] = useState(new Date().getFullYear());
  const [error, setError] = useState(""); const [busy, setBusy] = useState(false);
  const run = async (work: () => Promise<void>) => { setBusy(true); setError(""); try { await work(); } catch (e) { setError(e instanceof Error ? e.message : "Something went wrong."); } finally { setBusy(false); } };
  const load = () => run(async () => { const cgpa = await request<Cgpa>(`/api/students/${studentId}/cgpa`, token); setData(cgpa); });
  const createStudent = (event: FormEvent) => { event.preventDefault(); run(async () => { const student = await request<Student>("/api/students", token, { method: "POST", body: JSON.stringify({ fullName: name }) }); setStudentId(student.id); setData({ studentId: student.id, fullName: student.fullName, cgpa: 0, semesters: [] }); }); };
  const addSemester = (event: FormEvent) => { event.preventDefault(); run(async () => { await request(`/api/students/${studentId}/semesters`, token, { method: "POST", body: JSON.stringify({ name: semesterName, academicYear: year }) }); setSemesterName(""); await load(); }); };

  return <main>
    <header><span className="mark">G</span><div><h1>GradeGenie</h1><p>Your academic picture, in focus.</p></div></header>
    <section className="hero"><div><span className="eyebrow">CGPA PLANNER</span><h2>Turn every semester into a clearer next step.</h2><p>Save courses, track weighted GPA, and use your semester history to plan deliberately.</p></div><div className="score"><span>Current CGPA</span><strong>{data?.cgpa.toFixed(2) ?? "—"}</strong><small>out of 5.00</small></div></section>
    <section className="setup"><label>JWT access token<input value={token} onChange={e => setToken(e.target.value)} placeholder="Paste your bearer token" type="password" /></label><label>Existing student ID<input value={studentId} onChange={e => setStudentId(e.target.value)} placeholder="Student UUID" /></label><button onClick={load} disabled={busy || !token || !studentId}>Load record</button></section>
    {error && <p className="error">{error}</p>}
    {!data ? <form className="card" onSubmit={createStudent}><h3>Start your record</h3><p>Authenticated users create one personal academic profile.</p><input value={name} onChange={e => setName(e.target.value)} placeholder="Your full name" required /><button disabled={busy || !token}>Create student profile</button></form> : <>
      <section className="section-title"><div><span className="eyebrow">{data.fullName.toUpperCase()}</span><h3>Semester history</h3></div><span>{data.semesters.length} semester{data.semesters.length === 1 ? "" : "s"}</span></section>
      <div className="grid">{data.semesters.map(semester => <article className="card" key={semester.id}><div className="row"><h3>{semester.name}</h3><strong>{semester.gpa.toFixed(2)}</strong></div><p>{semester.academicYear} · {semester.totalCreditUnits} credit units</p>{semester.courses.length ? <ul>{semester.courses.map(course => <li key={course.id}><span>{course.code} · {course.title}</span><b>{course.grade}</b></li>)}</ul> : <p className="muted">No courses added yet.</p>}</article>)}</div>
      <form className="card inline" onSubmit={addSemester}><div><h3>Add a semester</h3><p>Courses can be added through the API until the next UI slice.</p></div><input value={semesterName} onChange={e => setSemesterName(e.target.value)} placeholder="e.g. First Semester" required /><input value={year} onChange={e => setYear(Number(e.target.value))} type="number" min="1900" required /><button disabled={busy}>Add semester</button></form>
    </>}
  </main>;
}
