import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ApiError, createAlbum } from "../api";
import { emptyAlbum } from "../albumDefaults";
import { AlbumForm } from "../components/AlbumForm";
import { StatusMessage } from "../components/StatusMessage";
import type { AlbumInput } from "../types";

export function CreateAlbumPage() {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState("");
  const [errors, setErrors] = useState<Record<string, string[]>>({});

  const submit = async (album: AlbumInput) => {
    setSubmitting(true); setMessage(""); setErrors({});
    try {
      await createAlbum(album);
      navigate("/");
    } catch (error) {
      if (error instanceof ApiError) setErrors(error.validationErrors);
      setMessage(error instanceof Error ? error.message : "Could not save the album.");
    } finally { setSubmitting(false); }
  };

  return <section className="form-shell"><h1>Add album</h1>{message && <StatusMessage message={message} />}<AlbumForm initialValue={emptyAlbum} submitLabel="Save album" submitting={submitting} errors={errors} onSubmit={submit} /></section>;
}
