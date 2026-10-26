import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { ApiError, getAlbum, updateAlbum } from "../api";
import { AlbumForm } from "../components/AlbumForm";
import { StatusMessage } from "../components/StatusMessage";
import type { Album, AlbumInput } from "../types";

export function EditAlbumPage() {
  const id = Number(useParams().id);
  const navigate = useNavigate();
  const [album, setAlbum] = useState<Album>();
  const [notFound, setNotFound] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState("");
  const [errors, setErrors] = useState<Record<string, string[]>>({});

  useEffect(() => {
    const controller = new AbortController();
    if (!Number.isInteger(id) || id < 1) { setNotFound(true); return () => controller.abort(); }
    getAlbum(id, controller.signal).then(setAlbum).catch((error: unknown) => {
      if (error instanceof DOMException && error.name === "AbortError") return;
      if (error instanceof ApiError && error.status === 404) setNotFound(true);
      else setMessage(error instanceof Error ? error.message : "Could not load the album.");
    });
    return () => controller.abort();
  }, [id]);

  const submit = async (value: AlbumInput) => {
    setSubmitting(true); setMessage(""); setErrors({});
    try { await updateAlbum(id, value); navigate("/"); }
    catch (error) {
      if (error instanceof ApiError && error.status === 404) setNotFound(true);
      else {
        if (error instanceof ApiError) setErrors(error.validationErrors);
        setMessage(error instanceof Error ? error.message : "Could not save the album.");
      }
    } finally { setSubmitting(false); }
  };

  if (notFound) return <section className="empty-state"><h1>Album not found</h1><Link className="button" to="/">Back to albums</Link></section>;
  if (!album) return <section className="form-shell"><h1>Edit album</h1>{message ? <StatusMessage message={message} /> : <p>Loading album…</p>}</section>;
  return <section className="form-shell"><h1>Edit album</h1>{message && <StatusMessage message={message} />}<AlbumForm initialValue={album} submitLabel="Save changes" submitting={submitting} errors={errors} onSubmit={submit} /></section>;
}
