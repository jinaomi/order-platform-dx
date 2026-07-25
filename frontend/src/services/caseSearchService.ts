import { axiosPrivate } from "../api/axios";

export const GetCaseList = async (payload) => {
  const searchCaseUrl = "/api/Case/getAll";
  return axiosPrivate
    .post(searchCaseUrl, payload)
    .then((response) => {
      return response.data;
    })
    .catch(() => {});
};
