import { axiosPrivate } from "../api/axios";
import { useEffect } from "react";
import useRefreshToken from "./useRefreshToken";
import useAuth from "./useAuth";
import { useNavigate } from "react-router-dom";

const useAxiosPrivate = () => {
  const refresh = useRefreshToken();
  const { auth } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    const requestIntercept = axiosPrivate.interceptors.request.use(
      (config) => {
        config.headers["Accept"] = `*/*`;
        if (!config.headers["Authorization"]) {
          config.headers["Authorization"] = `Bearer ${auth?.accessToken}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    const responseIntercept = axiosPrivate.interceptors.response.use((response) => {
      return response;
    }, async function (error) {
        const prevRequest = error?.config;
       
        // if (error?.response?.status === 403 && !prevRequest?.sent) {
        //   prevRequest.sent = true;
        //   const newAccessToken = await refresh();
        //   prevRequest.headers["Authorization"] = `Bearer ${newAccessToken}`;
        //   return axiosPrivate(prevRequest);
        // }
        if(!error.response){
          localStorage.removeItem("AuthToken");
          navigate('/login', { replace: true });
        }
        return Promise.reject(error);
      }
    );

    return () => {
      axiosPrivate.interceptors.request.eject(requestIntercept);
      axiosPrivate.interceptors.response.eject(responseIntercept);
    };
  }, [auth, refresh]);

  return axiosPrivate;
};

export default useAxiosPrivate;
