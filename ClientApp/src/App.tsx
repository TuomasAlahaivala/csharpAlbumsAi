import { useEffect, useState } from "react";
import { Link, Navigate, Route, Routes } from "react-router-dom";
import { ApiError, getCurrentUser, login, logout } from "./api";
import { LoginPage } from "./pages/LoginPage";
import { AlbumListPage } from "./pages/AlbumListPage";
import { CreateAlbumPage } from "./pages/CreateAlbumPage";
import { DeleteAlbumPage } from "./pages/DeleteAlbumPage";
import { EditAlbumPage } from "./pages/EditAlbumPage";

export default function App() {
  const [email, setEmail] = useState<string>();
  const [authStatus, setAuthStatus] = useState<"loading" | "authenticated" | "anonymous" | "error">("loading");
  const [authError, setAuthError] = useState<string>();

  useEffect(() => {
    const handleUnauthorized = () => {
      setEmail(undefined);
      setAuthError(undefined);
      setAuthStatus("anonymous");
    };
    window.addEventListener("auth:unauthorized", handleUnauthorized);

    getCurrentUser()
      .then((user) => {
        setEmail(user.email);
        setAuthStatus("authenticated");
      })
      .catch((error: unknown) => {
        if (error instanceof ApiError && error.status === 401) {
          setAuthStatus("anonymous");
        } else {
          setAuthError("Unable to verify your sign-in. Please refresh the page to try again.");
          setAuthStatus("error");
        }
      });

    return () => window.removeEventListener("auth:unauthorized", handleUnauthorized);
  }, []);

  async function handleLogin(userEmail: string, password: string) {
    await login(userEmail, password);
    setEmail(userEmail);
    setAuthError(undefined);
    setAuthStatus("authenticated");
  }

  async function handleLogout() {
    try {
      await logout();
      setEmail(undefined);
      setAuthStatus("anonymous");
    } catch {
      setAuthError("Unable to sign out. Please try again.");
    }
  }

  if (authStatus === "loading") {
    return <main className="page" aria-busy="true">Checking sign-in…</main>;
  }

  if (authStatus === "error") {
    return <main className="page"><p className="status-message" role="alert">{authError}</p></main>;
  }

  if (authStatus === "anonymous") {
    return <LoginPage onLogin={handleLogin} />;
  }

  return (
    <>
      <header className="site-header">
        <Link to="/" className="brand">Albums</Link>
        <div className="site-account">
          <span>{email}</span>
          <button className="button" type="button" onClick={handleLogout}>Sign out</button>
        </div>
      </header>
      <main className="page">
        {authError && <p className="status-message" role="alert">{authError}</p>}
        <Routes>
          <Route path="/" element={<AlbumListPage />} />
          <Route path="/Albums/Create" element={<CreateAlbumPage />} />
          <Route path="/Albums/Edit/:id" element={<EditAlbumPage />} />
          <Route path="/Albums/Delete/:id" element={<DeleteAlbumPage />} />
          <Route path="/login" element={<Navigate to="/" replace />} />
          <Route path="*" element={<section className="empty-state"><h1>Page not found</h1><Link className="button" to="/">Back to albums</Link></section>} />
        </Routes>
      </main>
    </>
  );
}
