import { createContext, useState } from "react";

const AuthContext = createContext({});

export const AuthProvider = ({ children }) => {
  let currentAuthToken = localStorage.getItem("AuthToken") ?? {};
  if (typeof currentAuthToken === "string") {
    currentAuthToken = JSON.parse(currentAuthToken);
  }
  const [auth, setAuth] = useState(currentAuthToken);

  return (
    <AuthContext.Provider value={{ auth, setAuth }}>
      {children}
    </AuthContext.Provider>
  );
};

export default AuthContext;
