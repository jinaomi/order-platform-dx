import { useState } from "react";

const FormInput = (props) => {
  const { label, onChange, children, isRequired, ...inputProps } = props;
  const [focused, setFocused] = useState(false);

  const handleFocus = () => {
    setFocused(true);
  };
  return (
    <div className="section-item">
      <label className="section-label">
        {label}
        {isRequired ? <span className="required-icon"> *</span> : null}
      </label>
      {props.type === "textarea" ? (
        <textarea
          {...inputProps}
          onChange={onChange}
          onBlur={handleFocus}
          focused={focused.toString()}
        />
      ) : (
        <input
          {...inputProps}
          onChange={onChange}
          onBlur={handleFocus}
          focused={focused.toString()}
        />
      )}
      {children}
    </div>
  );
};

export default FormInput;
