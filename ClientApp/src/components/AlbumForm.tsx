import { useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import type { AlbumInput } from "../types";

interface Props {
  initialValue: AlbumInput;
  submitLabel: string;
  submitting: boolean;
  errors: Record<string, string[]>;
  onSubmit: (album: AlbumInput) => Promise<void>;
}

export function AlbumForm({ initialValue, submitLabel, submitting, errors, onSubmit }: Props) {
  const [album, setAlbum] = useState(initialValue);
  const fieldError = (name: keyof AlbumInput) => errors[name]?.[0] ?? errors[name[0].toUpperCase() + name.slice(1)]?.[0];

  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    void onSubmit(album);
  };

  return (
    <form className="album-form" onSubmit={submit}>
      <label>
        <span>Title</span>
        <input required maxLength={120} value={album.title} onChange={(e) => setAlbum({ ...album, title: e.target.value })} />
        {fieldError("title") && <span className="field-validation">{fieldError("title")}</span>}
      </label>
      <label>
        <span>Artist</span>
        <input required maxLength={120} value={album.artist} onChange={(e) => setAlbum({ ...album, artist: e.target.value })} />
        {fieldError("artist") && <span className="field-validation">{fieldError("artist")}</span>}
      </label>
      <div className="form-grid">
        <label>
          <span>Release year</span>
          <input required type="number" min={1888} max={2100} value={album.releaseYear} onChange={(e) => setAlbum({ ...album, releaseYear: e.target.valueAsNumber })} />
          {fieldError("releaseYear") && <span className="field-validation">{fieldError("releaseYear")}</span>}
        </label>
        <label>
          <span>Format</span>
          <select value={album.format} onChange={(e) => setAlbum({ ...album, format: e.target.value })}>
            {['Vinyl', 'CD', 'Cassette', 'Digital', 'Other'].map((value) => <option key={value}>{value}</option>)}
          </select>
          {fieldError("format") && <span className="field-validation">{fieldError("format")}</span>}
        </label>
      </div>
      <div className="form-grid">
        <label>
          <span>Genre</span>
          <input required maxLength={80} value={album.genre} onChange={(e) => setAlbum({ ...album, genre: e.target.value })} />
          {fieldError("genre") && <span className="field-validation">{fieldError("genre")}</span>}
        </label>
        <label>
          <span>Condition</span>
          <select value={album.condition} onChange={(e) => setAlbum({ ...album, condition: e.target.value })}>
            {['Mint', 'Near mint', 'Very good', 'Good', 'Fair', 'Poor'].map((value) => <option key={value}>{value}</option>)}
          </select>
          {fieldError("condition") && <span className="field-validation">{fieldError("condition")}</span>}
        </label>
      </div>
      <label>
        <span>Notes</span>
        <textarea rows={4} maxLength={500} value={album.notes} onChange={(e) => setAlbum({ ...album, notes: e.target.value })} />
        {fieldError("notes") && <span className="field-validation">{fieldError("notes")}</span>}
      </label>
      <label className="checkbox">
        <input type="checkbox" checked={album.isFavorite} onChange={(e) => setAlbum({ ...album, isFavorite: e.target.checked })} />
        <span>Favorite album</span>
      </label>
      <div className="form-actions">
        <Link className="button" to="/">Cancel</Link>
        <button className="button primary" type="submit" disabled={submitting}>{submitting ? "Saving…" : submitLabel}</button>
      </div>
    </form>
  );
}
