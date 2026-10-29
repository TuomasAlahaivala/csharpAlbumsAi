import { cleanup, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { afterEach, describe, expect, it, vi } from "vitest";
import App from "./App";

afterEach(() => {
  cleanup();
  vi.restoreAllMocks();
});

describe("App", () => {
  it("loads and renders albums from the API", async () => {
    vi.spyOn(globalThis, "fetch")
      .mockResolvedValueOnce(new Response(JSON.stringify({ email: "admin@example.com" }), { status: 200, headers: { "Content-Type": "application/json" } }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ albums: [{ id: 1, title: "Kind of Blue", artist: "Miles Davis", releaseYear: 1959, format: "Vinyl", genre: "Jazz", condition: "Very good", isFavorite: true, notes: "Classic" }], availableFormats: ["Vinyl"] }), { status: 200, headers: { "Content-Type": "application/json" } }));
    render(<MemoryRouter initialEntries={["/"]}><App /></MemoryRouter>);
    expect(await screen.findByText("Kind of Blue")).toBeInTheDocument();
    expect(screen.getByText("Miles Davis")).toBeInTheDocument();
  });

  it("renders the create form route after checking the session", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValue(new Response(JSON.stringify({ email: "admin@example.com" }), { status: 200, headers: { "Content-Type": "application/json" } }));
    render(<MemoryRouter initialEntries={["/Albums/Create"]}><App /></MemoryRouter>);
    expect(await screen.findByRole("heading", { name: "Add album" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Save album" })).toBeInTheDocument();
  });

  it("shows the login form when there is no authenticated session", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValue(new Response(JSON.stringify({ title: "Unauthorized" }), {
      status: 401,
      headers: { "Content-Type": "application/problem+json" },
    }));
    render(<MemoryRouter><App /></MemoryRouter>);
    expect(await screen.findByRole("heading", { name: "Sign in" })).toBeInTheDocument();
    expect(screen.getByLabelText("Email")).toBeInTheDocument();
    expect(screen.getByLabelText("Password")).toBeInTheDocument();
  });

  it("signs in and loads the protected album page", async () => {
    const fetchMock = vi.spyOn(globalThis, "fetch")
      .mockResolvedValueOnce(new Response(JSON.stringify({ title: "Unauthorized" }), {
        status: 401,
        headers: { "Content-Type": "application/problem+json" },
      }))
      .mockResolvedValueOnce(new Response(null, { status: 204 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ albums: [], availableFormats: [] }), {
        status: 200,
        headers: { "Content-Type": "application/json" },
      }));
    const user = userEvent.setup();
    render(<MemoryRouter><App /></MemoryRouter>);

    await user.type(await screen.findByLabelText("Email"), "admin-123@example.com");
    await user.type(screen.getByLabelText("Password"), "strong-password");
    await user.click(screen.getByRole("button", { name: "Sign in" }));

    expect(await screen.findByText("No albums found")).toBeInTheDocument();
    expect(fetchMock.mock.calls[1][0]).toBe("/api/auth/login");
    expect(fetchMock.mock.calls[1][1]).toMatchObject({
      credentials: "same-origin",
      method: "POST",
      body: JSON.stringify({ email: "admin-123@example.com", password: "strong-password" }),
    });
  });
});
