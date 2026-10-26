import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { ApiError, deleteAlbum, getAlbum } from "../api";
import { StatusMessage } from "../components/StatusMessage";
import type { Album } from "../types";

export function DeleteAlbumPage() {
  const id = Number(useParams().id);
  const navigate = useNavigate();
  const [album, setAlbum] = useState<Album>();
  const [notFound, setNotFound] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [message, setMessage] = useState("");

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

  const remove = async () => {
    setDeleting(true); setMessage("");
    try { await deleteAlbum(id); navigate("/"); }
    catch (error) {
      if (error instanceof ApiError && error.status === 404) setNotFound(true);
      else setMessage(error instanceof Error ? error.message : "Could not delete the album.");
      setDeleting(false);
    }
  };

  if (notFound) return <section className="empty-state"><h1>Album not found</h1><Link className="button" to="/">Back to albums</Link></section>;
  return <section className="delete-shell"><h1>Delete album</h1>{message && <StatusMessage message={message} />}{album ? <><p>Remove <strong>{album.title}</strong> by {album.artist} from your collection?</p><div className="form-actions"><Link className="button" to="/">Cancel</Link><button className="button danger" type="button" disabled={deleting} onClick={() => void remove()}>{deleting ? "Deleting…" : "Delete album"}</button></div></> : !message && <p>Loading album…</p>}</section>;
}
