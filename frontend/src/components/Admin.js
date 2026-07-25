import { Link } from "react-router-dom";
import { Button, Box } from "@mui/material";
import Users from "./Users";

const Admin = () => {
  return (
    <section>
      <h1>Admins Page</h1>
      <Box sx={{ mb: 2 }}>
        <Button variant="contained" component={Link} to="/admin/templates">
          テンプレート管理
        </Button>
      </Box>
      <Users />
      <Box sx={{ mt: 2 }}>
        <Link to="/">Home</Link>
      </Box>
    </section>
  );
};

export default Admin;
