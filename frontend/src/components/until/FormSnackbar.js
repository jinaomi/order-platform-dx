import { Alert, Snackbar } from "@mui/material";
import { makeStyles } from "@mui/styles";

const useStyles = makeStyles({
  cookieAlert: {
    "& .MuiAlert-icon": {
      fontSize: 24,
      paddingTop: 10,
      paddingRight: 10,
    },
  },
});

const FormSnackbar = ({ item, setItem }) => {
  const classes = useStyles();
  return (
    //severity =  success || error || warning || info
    <>
      <Snackbar
        open={item.isOpen}
        autoHideDuration={5000}
        onClose={() => {
          return setItem({ ...item, isOpen: false });
        }}
      >
        <Alert
          className={classes.cookieAlert}
          variant="filled"
          onClose={() => {
            return setItem({ ...item, isOpen: false });
          }}
          severity={item.status}
          sx={{ width: "100%" }}
        >
          {item.message}
        </Alert>
      </Snackbar>
    </>
  );
};
export default FormSnackbar;
