import { useEffect, useState, type FormEvent } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { getAlbums } from "../api";
import { StatusMessage } from "../components/StatusMessage";
import type { AlbumListResponse } from "../types";

export function AlbumListPage() {
  const [params, setParams] = useSearchParams();
  const search = params.get("search") ?? "";
  const format = params.get("format") ?? "";
  const [searchInput, setSearchInput] = useState(search);
  const [formatInput, setFormatInput] = useState(format);
  const [result, setResult] = useState<AlbumListResponse>();
  const [error, setError] = useState("");

  useEffect(() => {
    setSearchInput(search);
    setFormatInput(format);
    const controller = new AbortController();
    setError("");
    getAlbums(search, format, controller.signal)
      .then(setResult)
      .catch((reason: unknown) => {
        if (reason instanceof DOMException && reason.name === "AbortError") return;
        setError(reason instanceof Error ? reason.message : "Could not load albums.");
      });
    return () => controller.abort();
  }, [search, format]);

  const applyFilters = (event: FormEvent) => {
    event.preventDefault();
    const next = new URLSearchParams();
    if (searchInput.trim()) next.set("search", searchInput.trim());
    if (formatInput) next.set("format", formatInput);
    setParams(next);
  };

  return (
    <>
      <section className="toolbar">
        <div>
          <h1>My Albums</h1>
          <p>{result ? `${result.albums.length} album${result.albums.length === 1 ? "" : "s"} in this view` : "Loading albums…"}</p>
        </div>
        <Link className="button primary" to="/Albums/Create">Add album</Link>
      </section>
      <form className="filters" onSubmit={applyFilters}>
        <label>
          <span>Search</span>
          <input type="search" value={searchInput} onChange={(e) => setSearchInput(e.target.value)} placeholder="Title, artist, or genre" />
        </label>
        <label>
          <span>Format</span>
          <select value={formatInput} onChange={(e) => setFormatInput(e.target.value)}>
            <option value="">All formats</option>
            {result?.availableFormats.map((value) => <option key={value}>{value}</option>)}
          </select>
        </label>
        <button className="button" type="submit">Filter</button>
      </form>
      {error && <StatusMessage message={error} />}
      {!error && !result && <div className="empty-state" aria-live="polite">Loading albums…</div>}
      {result?.albums.length === 0 && (
        <div className="empty-state">
          <h2>No albums found</h2>
          <p>Add a record you own or adjust the current filters.</p>
          <Link className="button primary" to="/Albums/Create">Add album</Link>
        </div>
      )}
      {result && result.albums.length > 0 && (
        <div className="album-grid">
          {result.albums.map((album) => (
            <article className="album-card" key={album.id}>
              <div className="album-card__header">
                <div><h2>{album.title}</h2><p>{album.artist}</p></div>
                {album.isFavorite && <span className="favorite" title="Favorite">Star</span>}
              </div>
              <dl>
                <div><dt>Year</dt><dd>{album.releaseYear}</dd></div>
                <div><dt>Format</dt><dd>{album.format}</dd></div>
                <div><dt>Genre</dt><dd>{album.genre}</dd></div>
                <div><dt>Condition</dt><dd>{album.condition}</dd></div>
              </dl>
              {album.notes && <p className="notes">{album.notes}</p>}
              <div className="card-actions">
                <Link className="button" to={`/Albums/Edit/${album.id}`}>Edit</Link>
                <Link className="button danger" to={`/Albums/Delete/${album.id}`}>Delete</Link>
              </div>
            </article>
          ))}
        </div>
      )}
    </>
  );
}
