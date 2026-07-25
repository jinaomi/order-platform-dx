import { Button, Tooltip } from "@mui/material";

const FormButton = ({ itemName, buttonType = "normal", ...props }) => {
  var styleMode;

  switch (buttonType) {
    case "delete":
    case "attach":
      styleMode = {
        backgroundColor: "#0B78D1",
        color: "#fff",
        backgroundHover: "#fff",
        colorHover: "#0B78D1",
        backgroundActive: "#095989",
        colorActive: "#fff",
      };
      break;
    case "cancel":
      styleMode = {
        backgroundColor: "black",
        color: "white",
        backgroundHover: "white",
        colorHover: "black",
        backgroundActive: "#c9c9c9",
        colorActive: "white",
      };
      break;
    default:
      styleMode = {
        backgroundColor: "#14873A",
        color: "#fff",
        backgroundHover: "#fff",
        colorHover: "#14873A",
        backgroundActive: "#C0E8CE",
        colorActive: "#fff",
      };
      break;
  }

  const styleButton = {
    backgroundColor: styleMode.backgroundColor,
    color: styleMode.color,
    "&:hover": {
      backgroundColor: styleMode.backgroundHover,
      color: styleMode.colorHover,
      border: "1px solid " + styleMode.backgroundColor,
    },
    "&:active": {
      backgroundColor: styleMode.backgroundActive,
      color: styleMode.colorActive,
    },
    padding: "10px",
    borderRadius: "5px",
    boxShadow: "0 4px 8px rgba(0, 0, 0, 0.5)",
    transition: "background-color 0.3s, box-shadow 0.5s",
    cursor: "pointer",
    fontSize: "30px",
    lineHeight: "1.3",
    border: "none",
    width: "100%",
  };

  return (
    <Tooltip title={props.titleContent} placement="top">
      <span className="tooltipSpan">
        <Button sx={styleButton} {...props}>
          {itemName}
        </Button>
      </span>
    </Tooltip>
  );
};

export default FormButton;
