import type { AlbumInput } from "./types";

export const emptyAlbum: AlbumInput = {
  title: "",
  artist: "",
  releaseYear: new Date().getFullYear(),
  format: "Vinyl",
  genre: "",
  condition: "Good",
  isFavorite: false,
  notes: "",
};
