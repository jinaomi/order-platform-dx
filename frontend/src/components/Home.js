import { useNavigate, Link } from "react-router-dom";
import { useContext } from "react";
import AuthContext from "../context/AuthProvider";

const Home = () => {
  const { setAuth } = useContext(AuthContext);
  const navigate = useNavigate();

  const logout = async () => {
    // if used in more components, this should be in context
    // axios to /logout endpoint
    
    setAuth({});
    navigate("/linkpage");
  };

  return (
    <section className="select-form">
      <h1>Home</h1>
      <br />
      <Link to="/customermanagement">Customer</Link>
      <br />
      <Link to="/casemanagement">Case</Link>
      <br />
      <Link to="/company">Company</Link>
      <br />
      <Link to="/document">Document Search</Link>
      <div className="flexGrow">
        <button onClick={logout}>Sign Out</button>
      </div>
    </section>
  );
};

export default Home;
