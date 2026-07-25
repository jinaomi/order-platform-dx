import LoadingSpinner from "./until/LoadingSpinner.js";
import { useRef, useState, useEffect } from "react";
import useAuth from "../hooks/useAuth";
import { Link, useNavigate, useLocation } from "react-router-dom";

import axios from "../api/axios";
import FormButton from "./until/FormButton";
const LOGIN_URL = "/api/Account/login";

const Login = () => {
  const [loading, setLoading] = useState(false);

  const { setAuth } = useAuth();

  const navigate = useNavigate();
  const location = useLocation();
  const from = location.state?.from?.pathname || "/";

  const userRef = useRef();
  const errRef = useRef();

  const [username, setUser] = useState("");
  const [password, setPwd] = useState("");
  const [errMsg, setErrMsg] = useState("");

  useEffect(() => {
    setLoading(false);
    userRef.current.focus();
  }, []);

  useEffect(() => {
    setErrMsg("");
  }, [username, password]);

  const handleSubmit = async (e) => {
    setLoading(true);
    e.preventDefault();

    try {
      setLoading(true);

      const response = await axios.post(
        LOGIN_URL,
        JSON.stringify({ username, password }),
        {
          headers: {
            "Content-Type": "application/json",
          },
          withCredentials: true,
        }
      );
      const accessToken = response?.data?.accessToken;
      const roles = response?.data?.roles;
      var tokenStorage = {
        accessToken: accessToken,
        roles: roles,
        username: username,
        password: password,
      };
      localStorage.setItem("AuthToken", JSON.stringify(tokenStorage));
      setAuth({ username, password, roles, accessToken });
      setUser("");
      setPwd("");
      navigate(from, { replace: true });
      setLoading(false);
    } catch (err) {
      setLoading(false);
      if (!err?.response) {
        setErrMsg("サーバーから応答がありません");
      } else if (err.response?.status === 400) {
        setErrMsg("ユーザー名またはパスワードが正しくありません。");
      } else if (err.response?.status === 401) {
        setErrMsg("ユーザー名またはパスワードが正しくありません。");
      } else {
        setErrMsg("ログインに失敗しました");
      }
      errRef.current.focus();
    } finally {
      setLoading(false);
    }
  };

  return (
    <section
      className="select-form"
      style={{ borderStyle: "solid", maxWidth: "600px" }}
    >
      <p
        ref={errRef}
        className={errMsg ? "errmsg" : "offscreen"}
        aria-live="assertive"
      >
        {errMsg}
      </p>

      <LoadingSpinner loading={loading}></LoadingSpinner>

      <form onSubmit={handleSubmit}>
        <label htmlFor="username">ユーザー名を入力してください</label>
        <input
          type="text"
          id="username"
          ref={userRef}
          autoComplete="off"
          onChange={(e) => setUser(e.target.value)}
          value={username}
          required
        />
        <label htmlFor="password" style={{ marginTop: "1rem" }}>
          パスワードを入力してください
        </label>
        <input
          type="password"
          id="password"
          onChange={(e) => setPwd(e.target.value)}
          value={password}
          required
        />
        <br />
        {/* <button>ログイン</button> */}
        <FormButton itemName="ログイン" type="submit" />
      </form>
      <div className="version-info">Version 24.1.1000 Powered by ITFreee</div>
      {/* <p>
        Need an Account?
        <br />
        <span className="line">
          <Link to="/register">Sign Up</Link>
        </span>
      </p> */}
    </section>
  );
};

export default Login;
