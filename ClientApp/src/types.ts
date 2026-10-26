export interface AlbumInput {
  title: string;
  artist: string;
  releaseYear: number;
  format: string;
  genre: string;
  condition: string;
  isFavorite: boolean;
  notes: string;
}

export interface Album extends AlbumInput {
  id: number;
}

export interface AlbumListResponse {
  albums: Album[];
  availableFormats: string[];
}

export interface ValidationProblem {
  title?: string;
  errors?: Record<string, string[]>;
}
