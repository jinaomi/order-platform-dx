const userService = {
  getAll: (axiosPrivate) => axiosPrivate.get('/api/account/users'),
  create: (axiosPrivate, data) => axiosPrivate.post('/api/account/register-user', data),
  deleteById: (axiosPrivate, id) => axiosPrivate.delete(`/api/account/users/${id}`),
};

export default userService;
