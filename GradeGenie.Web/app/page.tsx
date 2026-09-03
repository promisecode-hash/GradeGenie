"use client";

import { FormEvent, useMemo, useState } from "react";

type Student = { id: string; fullName: string; institutionType: number };
type Course = { id: string; code: string; title: string; creditUnits: number; grade: string; gradePoint: number };
type Semester = { id: string; name: string; academicYear: number; gpa: number; totalCreditUnits: number; courses: Course[] };
type Cgpa = { studentId: string; fullName: string; cgpa: number; institutionType: number; semesters: Semester[] };
type TargetGradeResult = { requiredGradePoint: number; requiredLetterGrade: string };
type ScaleConversionResult = { convertedValue: number; sourceScale: number; targetScale: number };
type AcademicPlanResult = { requiredGradePoint: number; recommendedPriority: string; summary: string };
type SemesterInsightResult = { semesterId: string; gpa: number; insight: string };
const apiBase = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000";

const institutionLabel = (institutionType: number) => institutionType === 1 ? "Polytechnic" : "University";
const gradeOptions = ["A", "B", "C", "D", "E", "F"];

async function request<T>(path: string, token: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBase}${path}`, { ...init, headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}`, ...init?.headers } });
  if (!response.ok) throw new Error(response.status === 404 ? "Record not found or not owned by this account." : `Request failed (${response.status}).`);
  return response.json() as Promise<T>;
}

export default function Home() {
  const [token, setToken] = useState(""); const [studentId, setStudentId] = useState(""); const [name, setName] = useState("");
  const [institutionType, setInstitutionType] = useState<number>(0);
  const [data, setData] = useState<Cgpa | null>(null); const [semesterName, setSemesterName] = useState(""); const [year, setYear] = useState(new Date().getFullYear());
  const [courseCode, setCourseCode] = useState(""); const [courseTitle, setCourseTitle] = useState(""); const [courseUnits, setCourseUnits] = useState(3);
  const [courseGrade, setCourseGrade] = useState("A"); const [targetCurrent, setTargetCurrent] = useState(3.0); const [targetDesired, setTargetDesired] = useState(3.5);
  const [targetCompleted, setTargetCompleted] = useState(20); const [targetRemaining, setTargetRemaining] = useState(20); const [scaleValue, setScaleValue] = useState(75);
  const [sourceScale, setSourceScale] = useState(4); const [targetScale, setTargetScale] = useState(1); const [targetResult, setTargetResult] = useState<TargetGradeResult | null>(null);
  const [conversionResult, setConversionResult] = useState<ScaleConversionResult | null>(null); const [planResult, setPlanResult] = useState<AcademicPlanResult | null>(null);
  const [error, setError] = useState(""); const [busy, setBusy] = useState(false);

  const activeSemesterId = useMemo(() => data?.semesters[0]?.id ?? "", [data]);

  const run = async (work: () => Promise<void>) => { setBusy(true); setError(""); try { await work(); } catch (e) { setError(e instanceof Error ? e.message : "Something went wrong."); } finally { setBusy(false); } };
  const load = () => run(async () => { const cgpa = await request<Cgpa>(`/api/students/${studentId}/cgpa`, token); setData(cgpa); });
  const createStudent = (event: FormEvent) => { event.preventDefault(); run(async () => { const student = await request<Student>("/api/students", token, { method: "POST", body: JSON.stringify({ fullName: name, institutionType }) }); setStudentId(student.id); setData({ studentId: student.id, fullName: student.fullName, cgpa: 0, institutionType: student.institutionType, semesters: [] }); }); };
  const addSemester = (event: FormEvent) => { event.preventDefault(); run(async () => { await request(`/api/students/${studentId}/semesters`, token, { method: "POST", body: JSON.stringify({ name: semesterName, academicYear: year }) }); setSemesterName(""); await load(); }); };
  const addCourse = (event: FormEvent) => { event.preventDefault(); run(async () => { if (!activeSemesterId) throw new Error("Create a semester before adding a course."); await request(`/api/students/${studentId}/semesters/${activeSemesterId}/courses`, token, { method: "POST", body: JSON.stringify({ code: courseCode, title: courseTitle, creditUnits: courseUnits, grade: gradeOptions.indexOf(courseGrade) }) }); setCourseCode(""); setCourseTitle(""); setCourseUnits(3); setCourseGrade("A"); await load(); }); };
  const computeTarget = () => run(async () => { const result = await request<TargetGradeResult>(`/api/students/${studentId}/target-grade`, token, { method: "POST", body: JSON.stringify({ currentCgpa: Number(targetCurrent), targetCgpa: Number(targetDesired), completedCreditUnits: Number(targetCompleted), remainingCreditUnits: Number(targetRemaining) }) }); setTargetResult(result); });
  const computePlan = () => run(async () => { const result = await request<AcademicPlanResult>(`/api/students/${studentId}/academic-plan`, token, { method: "POST", body: JSON.stringify({ currentCgpa: Number(targetCurrent), targetCgpa: Number(targetDesired), completedCreditUnits: Number(targetCompleted), remainingCreditUnits: Number(targetRemaining) }) }); setPlanResult(result); });
  const convertScale = () => run(async () => { const result = await request<ScaleConversionResult>("/api/conversion/convert", token, { method: "POST", body: JSON.stringify({ value: Number(scaleValue), sourceScale: Number(sourceScale), targetScale: Number(targetScale) }) }); setConversionResult(result); });
  const generateInsight = (semesterId: string) => run(async () => {
    const result = await request<SemesterInsightResult>(`/api/students/${studentId}/semesters/${semesterId}/insight`, token, { method: "POST" });
    setSemesterInsights(previous => ({ ...previous, [semesterId]: result.insight }));
  });

  const copyStudentId = async () => {
    if (!data?.studentId) return;
    try { await navigator.clipboard.writeText(data.studentId); } catch { /* ignore */ }
  };

  return (
    <main className="page-shell">
      <header className="topbar">
        <div className="brand-wrap">
          <span className="mark">G</span>
          <div>
            <h1>GradeGenie</h1>
            <p>Your academic picture, in focus.</p>
          </div>
        </div>
      </header>

      <section className="hero panel">
        <div className="hero-copy">
          <span className="eyebrow">CGPA PLANNER</span>
          <h2>Turn every semester into a clearer next step.</h2>
          <p>Save courses, track weighted GPA, and use your semester history to plan deliberately.</p>
        </div>
        <div className="score-panel">
          <span>Current CGPA</span>
          <strong>{data?.cgpa.toFixed(2) ?? "—"}</strong>
          <small>Weighted average</small>
        </div>
      </section>

      <section className="auth-panel panel">
        <label className="field-group">
          <span>JWT access token</span>
          <input value={token} onChange={e => setToken(e.target.value)} placeholder="Paste your bearer token" type="password" />
        </label>
        <label className="field-group">
          <span>Existing student ID</span>
          <input value={studentId} onChange={e => setStudentId(e.target.value)} placeholder="Student UUID" />
        </label>
        <button onClick={load} disabled={busy || !token || !studentId} className="primary-button" type="button">Load record</button>
      </section>

      {error && <p className="error-banner">{error}</p>}

      {!data ? (
        <form className="panel spotlight-card" onSubmit={createStudent}>
          <div className="section-head compact">
            <div>
              <span className="eyebrow">START</span>
              <h3>Build your academic profile</h3>
            </div>
          </div>

          <div className="field-grid single-col">
            <label className="field-group">
              <span>Full name</span>
              <input value={name} onChange={e => setName(e.target.value)} placeholder="Your full name" required />
            </label>
            <label className="field-group">
              <span>Institution type</span>
              <select value={institutionType} onChange={e => setInstitutionType(Number(e.target.value))}>
                <option value={0}>University</option>
                <option value={1}>Polytechnic</option>
              </select>
            </label>
          </div>

          <button className="primary-button" disabled={busy}>Create student profile</button>
        </form>
      ) : (
        <>
          <section className="summary-grid">
            <article className="stat-card panel">
              <span className="metric-label">Student</span>
              <strong>{data.fullName}</strong>
              <small>{institutionLabel(data.institutionType)}</small>
            </article>
            <article className="stat-card panel">
              <span className="metric-label">Semesters</span>
              <strong>{data.semesters.length}</strong>
              <small>Recorded</small>
            </article>
            <article className="stat-card panel">
              <span className="metric-label">CGPA</span>
              <strong>{data.cgpa.toFixed(2)}</strong>
              <small>Current score</small>
            </article>
          </section>

          <section className="page-section">
            <div className="section-head">
              <div>
                <span className="eyebrow">PROFILE</span>
                <h3>Semester history</h3>
              </div>
              <span className="section-meta">{institutionLabel(data.institutionType)} · {data.semesters.length} semester{data.semesters.length === 1 ? "" : "s"}</span>
            </div>

            <div className="student-box panel">
              <div>
                <small>Student ID</small>
                <div className="student-id">{data.studentId}</div>
              </div>
              <button type="button" className="secondary-button" onClick={copyStudentId}>Copy ID</button>
            </div>

            <div className="semester-grid">
              {data.semesters.map(semester => (
                <article className="panel semester-card" key={semester.id}>
                  <div className="row-between">
                    <h3>{semester.name}</h3>
                    <strong>{semester.gpa.toFixed(2)}</strong>
                  </div>
                  <p className="muted">{semester.academicYear} · {semester.totalCreditUnits} credit units</p>

                  {semester.courses.length ? (
                    <ul className="course-list">
                      {semester.courses.map(course => (
                        <li key={course.id}>
                          <span>{course.code} · {course.title}</span>
                          <b>{course.grade}</b>
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <p className="muted empty-state">No courses added yet.</p>
                  )}

                  {semesterInsights[semester.id] ? (
                    <p className="result-box">{semesterInsights[semester.id]}</p>
                  ) : (
                    <button type="button" className="secondary-button insight-button" onClick={() => generateInsight(semester.id)} disabled={busy}>Generate insight</button>
                  )}
                </article>
              ))}
            </div>

            <form className="panel add-form" onSubmit={addSemester}>
              <div className="section-head compact">
                <div>
                  <span className="eyebrow">NEW</span>
                  <h3>Add a semester</h3>
                </div>
              </div>

              <div className="field-grid">
                <label className="field-group">
                  <span>Semester name</span>
                  <input value={semesterName} onChange={e => setSemesterName(e.target.value)} placeholder="e.g. First Semester" required />
                </label>
                <label className="field-group">
                  <span>Academic year</span>
                  <input value={year} onChange={e => setYear(Number(e.target.value))} type="number" min="1900" required />
                </label>
              </div>

              <button className="primary-button" disabled={busy}>Add semester</button>
            </form>

            <form className="panel add-form" onSubmit={addCourse}>
              <div className="section-head compact">
                <div>
                  <span className="eyebrow">NEW</span>
                  <h3>Add a course</h3>
                </div>
              </div>

              <div className="field-grid">
                <label className="field-group">
                  <span>Course code</span>
                  <input value={courseCode} onChange={e => setCourseCode(e.target.value)} placeholder="Course code" required />
                </label>
                <label className="field-group">
                  <span>Course title</span>
                  <input value={courseTitle} onChange={e => setCourseTitle(e.target.value)} placeholder="Course title" required />
                </label>
                <label className="field-group">
                  <span>Credit units</span>
                  <input value={courseUnits} onChange={e => setCourseUnits(Number(e.target.value))} type="number" min="1" step="0.5" required />
                </label>
                <label className="field-group">
                  <span>Grade</span>
                  <select value={courseGrade} onChange={e => setCourseGrade(e.target.value)}>
                    {gradeOptions.map(grade => <option key={grade} value={grade}>{grade}</option>)}
                  </select>
                </label>
              </div>

              <button className="primary-button" disabled={busy || !activeSemesterId}>Add course</button>
            </form>
          </section>
        </>
      )}

      {data && (
        <section className="tools-grid">
          <div className="panel tool-card">
            <div className="section-head compact">
              <div>
                <span className="eyebrow">PLAN</span>
                <h3>Target grade planner</h3>
              </div>
            </div>

            <div className="field-grid">
              <label className="field-group">
                <span>Current CGPA</span>
                <input value={targetCurrent} onChange={e => setTargetCurrent(Number(e.target.value))} type="number" step="0.1" placeholder="Current CGPA" />
              </label>
              <label className="field-group">
                <span>Target CGPA</span>
                <input value={targetDesired} onChange={e => setTargetDesired(Number(e.target.value))} type="number" step="0.1" placeholder="Target CGPA" />
              </label>
              <label className="field-group">
                <span>Completed credits</span>
                <input value={targetCompleted} onChange={e => setTargetCompleted(Number(e.target.value))} type="number" step="1" placeholder="Completed credits" />
              </label>
              <label className="field-group">
                <span>Remaining credits</span>
                <input value={targetRemaining} onChange={e => setTargetRemaining(Number(e.target.value))} type="number" step="1" placeholder="Remaining credits" />
              </label>
            </div>

            <div className="button-row">
              <button type="button" className="primary-button" onClick={computeTarget} disabled={busy}>Check required grade</button>
              <button type="button" className="secondary-button" onClick={computePlan} disabled={busy}>Plan strategy</button>
            </div>

            {targetResult && (
              <p className="result-box">Required grade point: <strong>{targetResult.requiredGradePoint.toFixed(2)}</strong> ({targetResult.requiredLetterGrade})</p>
            )}
            {planResult && <p className="result-box">{planResult.summary}</p>}
          </div>

          <div className="panel tool-card">
            <div className="section-head compact">
              <div>
                <span className="eyebrow">CONVERT</span>
                <h3>Scale converter</h3>
              </div>
            </div>

            <div className="field-grid">
              <label className="field-group">
                <span>Value</span>
                <input value={scaleValue} onChange={e => setScaleValue(Number(e.target.value))} type="number" step="0.1" placeholder="Value" />
              </label>
              <label className="field-group">
                <span>From scale</span>
                <select value={sourceScale} onChange={e => setSourceScale(Number(e.target.value))}>
                  <option value={4}>4-point</option>
                  <option value={5}>5-point</option>
                  <option value={10}>10-point</option>
                  <option value={12}>12-point</option>
                  <option value={100}>Percentage</option>
                </select>
              </label>
              <label className="field-group">
                <span>To scale</span>
                <select value={targetScale} onChange={e => setTargetScale(Number(e.target.value))}>
                  <option value={4}>4-point</option>
                  <option value={5}>5-point</option>
                  <option value={10}>10-point</option>
                  <option value={12}>12-point</option>
                  <option value={100}>Percentage</option>
                </select>
              </label>
            </div>

            <button type="button" className="primary-button" onClick={convertScale} disabled={busy}>Convert</button>
            {conversionResult && <p className="result-box">Converted value: <strong>{conversionResult.convertedValue.toFixed(2)}</strong></p>}
          </div>
        </section>
      )}
    </main>
  );
}
