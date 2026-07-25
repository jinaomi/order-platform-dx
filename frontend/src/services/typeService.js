const typeService = {
  getAll: (axiosPrivate) => axiosPrivate.get('/api/type/type'),
};
export default typeService;
