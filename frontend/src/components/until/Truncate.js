function Truncate(propString) {
  function truncate(str) {
    return str;
    // return str && str.length > propString.maxLength
    //   ? str.substring(0, propString.maxLength - 3) + "..."
    //   : str;
  }

  return <span style={propString.style}>{truncate(propString.str)}</span>;
}

export default Truncate;
