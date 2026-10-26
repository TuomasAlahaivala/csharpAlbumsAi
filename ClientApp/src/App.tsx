import { Link, Route, Routes } from "react-router-dom";
import { AlbumListPage } from "./pages/AlbumListPage";
import { CreateAlbumPage } from "./pages/CreateAlbumPage";
import { DeleteAlbumPage } from "./pages/DeleteAlbumPage";
import { EditAlbumPage } from "./pages/EditAlbumPage";

export default function App() {
  return (
    <>
      <header className="site-header">
        <Link to="/" className="brand">Albums</Link>
      </header>
      <main className="page">
        <Routes>
          <Route path="/" element={<AlbumListPage />} />
          <Route path="/Albums/Create" element={<CreateAlbumPage />} />
          <Route path="/Albums/Edit/:id" element={<EditAlbumPage />} />
          <Route path="/Albums/Delete/:id" element={<DeleteAlbumPage />} />
          <Route path="*" element={<section className="empty-state"><h1>Page not found</h1><Link className="button" to="/">Back to albums</Link></section>} />
        </Routes>
      </main>
    </>
  );
}
