import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from "@mui/material";
import useAxiosPrivate from "../../hooks/useAxiosPrivate";
import userService from "../../services/userService";
import FormSnackbar from "../../components/until/FormSnackbar";
import LoadingSpinner from "../../components/until/LoadingSpinner";

const emptyForm = {
  username: "",
  email: "",
  password: "",
  role: "User",
};

const UserManagement = () => {
  const axiosPrivate = useAxiosPrivate();
  const navigate = useNavigate();

  const [loading, setLoading] = useState(false);
  const [users, setUsers] = useState([]);
  const [snackbar, setSnackbar] = useState({ isOpen: false, status: "success", message: "" });
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [formData, setFormData] = useState(emptyForm);
  const [deleteTarget, setDeleteTarget] = useState(null);

  const fetchUsers = async () => {
    setLoading(true);
    try {
      const response = await userService.getAll(axiosPrivate);
      setUsers(response.data ?? []);
    } catch {
      setSnackbar({ isOpen: true, status: "error", message: "エラーが発生しました。再試行するか、サポートにお問い合わせください。" });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  const handleOpenCreate = () => {
    setFormData(emptyForm);
    setCreateDialogOpen(true);
  };

  const handleCloseCreate = () => {
    setCreateDialogOpen(false);
  };

  const handleFormChange = (field, value) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleCreate = async () => {
    if (!formData.username.trim() || !formData.email.trim() || !formData.password.trim()) return;
    setLoading(true);
    try {
      await userService.create(axiosPrivate, {
        Username: formData.username,
        Email: formData.email,
        Password: formData.password,
        ConfirmPassword: formData.password,
        Role: formData.role,
      });
      setCreateDialogOpen(false);
      setSnackbar({ isOpen: true, status: "success", message: "ユーザーを作成しました。" });
      await fetchUsers();
    } catch (error) {
      const msg = error.response?.data?.message ?? error.response?.data?.Message ?? "エラーが発生しました。再試行するか、サポートにお問い合わせください。";
      setSnackbar({ isOpen: true, status: "error", message: msg });
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    setLoading(true);
    try {
      await userService.deleteById(axiosPrivate, deleteTarget.id);
      setUsers((prev) => prev.filter((u) => u.id !== deleteTarget.id));
      setDeleteTarget(null);
      setSnackbar({ isOpen: true, status: "success", message: "ユーザーを削除しました。" });
    } catch {
      setSnackbar({ isOpen: true, status: "error", message: "削除に失敗しました。" });
    } finally {
      setLoading(false);
    }
  };

  const roleLabel = (role) => {
    if (role === "Admin") return "管理者";
    if (role === "User") return "一般ユーザー";
    return role;
  };

  return (
    <section>
      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 2 }}>
        <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
          <Button variant="text" onClick={() => navigate("/")}>← 戻る</Button>
          <Typography variant="h5">ユーザー管理</Typography>
        </Box>
        <Button variant="contained" onClick={handleOpenCreate}>
          + ユーザー追加
        </Button>
      </Box>

      {users.length === 0 && !loading ? (
        <Typography>ユーザーがいません</Typography>
      ) : (
        <TableContainer component={Paper}>
          <Table sx={{ minWidth: 600 }} aria-label="user table">
            <TableHead>
              <TableRow>
                <TableCell>ユーザー名</TableCell>
                <TableCell>メールアドレス</TableCell>
                <TableCell style={{ textAlign: "center" }}>ロール</TableCell>
                <TableCell style={{ textAlign: "center" }}>操作</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {users.map((row) => (
                <TableRow key={row.id} sx={{ "&:last-child td, &:last-child th": { border: 0 } }}>
                  <TableCell>{row.userName}</TableCell>
                  <TableCell>{row.email}</TableCell>
                  <TableCell style={{ textAlign: "center" }}>{roleLabel(row.role)}</TableCell>
                  <TableCell style={{ textAlign: "center" }}>
                    <Button
                      variant="outlined"
                      color="error"
                      size="small"
                      onClick={() => setDeleteTarget(row)}
                    >
                      削除
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Dialog open={createDialogOpen} onClose={handleCloseCreate} fullWidth maxWidth="sm">
        <DialogTitle>新規ユーザー作成</DialogTitle>
        <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2, pt: "16px !important" }}>
          <TextField
            label="ユーザー名"
            fullWidth
            required
            value={formData.username}
            onChange={(e) => handleFormChange("username", e.target.value)}
          />
          <TextField
            label="メールアドレス"
            type="email"
            fullWidth
            required
            value={formData.email}
            onChange={(e) => handleFormChange("email", e.target.value)}
          />
          <TextField
            label="パスワード"
            type="password"
            fullWidth
            required
            value={formData.password}
            onChange={(e) => handleFormChange("password", e.target.value)}
          />
          <FormControl fullWidth required>
            <InputLabel id="role-select-label">ロール</InputLabel>
            <Select
              labelId="role-select-label"
              label="ロール"
              value={formData.role}
              onChange={(e) => handleFormChange("role", e.target.value)}
            >
              <MenuItem value="Admin">管理者 (Admin)</MenuItem>
              <MenuItem value="User">一般ユーザー (User)</MenuItem>
            </Select>
          </FormControl>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseCreate}>キャンセル</Button>
          <Button
            variant="contained"
            onClick={handleCreate}
            disabled={!formData.username.trim() || !formData.email.trim() || !formData.password.trim()}
          >
            作成
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={!!deleteTarget} onClose={() => setDeleteTarget(null)}>
        <DialogTitle>ユーザーを削除しますか？</DialogTitle>
        <DialogContent>
          <Typography>「{deleteTarget?.userName}」を削除します。この操作は元に戻せません。</Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteTarget(null)}>キャンセル</Button>
          <Button variant="contained" color="error" onClick={handleDelete}>削除</Button>
        </DialogActions>
      </Dialog>

      <LoadingSpinner loading={loading} />
      <FormSnackbar item={snackbar} setItem={setSnackbar} />
    </section>
  );
};

export default UserManagement;
