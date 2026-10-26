import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { afterEach, describe, expect, it, vi } from "vitest";
import App from "./App";

afterEach(() => vi.restoreAllMocks());

describe("App", () => {
  it("loads and renders albums from the API", async () => {
    vi.spyOn(globalThis, "fetch").mockResolvedValue(new Response(JSON.stringify({ albums: [{ id: 1, title: "Kind of Blue", artist: "Miles Davis", releaseYear: 1959, format: "Vinyl", genre: "Jazz", condition: "Very good", isFavorite: true, notes: "Classic" }], availableFormats: ["Vinyl"] }), { status: 200, headers: { "Content-Type": "application/json" } }));
    render(<MemoryRouter initialEntries={["/"]}><App /></MemoryRouter>);
    expect(await screen.findByText("Kind of Blue")).toBeInTheDocument();
    expect(screen.getByText("Miles Davis")).toBeInTheDocument();
  });

  it("renders the create form route", () => {
    render(<MemoryRouter initialEntries={["/Albums/Create"]}><App /></MemoryRouter>);
    expect(screen.getByRole("heading", { name: "Add album" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Save album" })).toBeInTheDocument();
  });
});
