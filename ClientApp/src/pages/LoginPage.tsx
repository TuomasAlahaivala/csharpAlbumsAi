import { useState, type FormEvent } from "react";
import { ApiError } from "../api";

type LoginPageProps = {
  onLogin: (email: string, password: string) => Promise<void>;
};

export function LoginPage({ onLogin }: LoginPageProps) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string>();
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(undefined);
    setIsSubmitting(true);

    try {
      await onLogin(email, password);
    } catch (loginError: unknown) {
      setError(loginError instanceof ApiError && loginError.status === 401
        ? "Invalid email or password."
        : "Unable to sign in. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="page login-page">
      <section className="form-shell login-shell">
        <h1>Sign in</h1>
        <p>Sign in to manage your album collection.</p>
        <form className="login-form" onSubmit={handleSubmit}>
          <label>
            <span>Email</span>
            <input
              type="email"
              autoComplete="username"
              required
              maxLength={320}
              value={email}
              onChange={(event) => setEmail(event.target.value)}
            />
          </label>
          <label>
            <span>Password</span>
            <input
              type="password"
              autoComplete="current-password"
              required
              maxLength={1024}
              value={password}
              onChange={(event) => setPassword(event.target.value)}
            />
          </label>
          {error && <p className="field-validation" role="alert">{error}</p>}
          <button className="button primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Signing in…" : "Sign in"}
          </button>
        </form>
      </section>
    </main>
  );
}
