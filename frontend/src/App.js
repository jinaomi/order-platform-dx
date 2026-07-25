import Register from "./components/Register";
import Login from "./components/Login";
import Home from "./components/Home";
import Layout from "./components/Layout";
import Admin from "./components/Admin";
import Missing from "./components/Missing";
import Unauthorized from "./components/Unauthorized";
import LinkPage from "./components/LinkPage";
import RequireAuth from "./components/RequireAuth";
import { Routes, Route } from "react-router-dom";
import Sidebar from "./components/Sidebar";
import TemplateList from "./pages/admin/TemplateList";
import KeywordBuilder from "./pages/admin/KeywordBuilder";
import UserManagement from "./pages/admin/UserManagement";

const ROLES = {
  User: "User",
  Editor: "Editor",
  Admin: "Admin",
  SuperAdmin: "SuperAdmin",
};

function App() {
  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        {/* public routes */}
        <Route path="login" element={<Login />} />
        <Route path="register" element={<Register />} />
        <Route path="linkpage" element={<LinkPage />} />
        <Route path="unauthorized" element={<Unauthorized />} />
        <Route path="/main" element={<Sidebar />} />
        <Route
          element={<RequireAuth allowedRoles={[ROLES.Admin, ROLES.User, ROLES.SuperAdmin]} />}
        >
          <Route path="/" element={<Sidebar />} />
          <Route path="/home" element={<Home />} />
          <Route path="/main" element={<Sidebar />} />
          <Route path="admin" element={<Admin />} />
          <Route path="/admin/templates" element={<TemplateList />} />
          <Route path="/admin/templates/:templateId/keywords" element={<KeywordBuilder />} />
          <Route path="/admin/users" element={<UserManagement />} />
        </Route>
        {/* catch all */}
        <Route path="*" element={<Missing />} />
      </Route>
    </Routes>
  );
}

export default App;
