const concatCustomerInfo = (stateProvince, city, street, buildingName, roomNumber) => {
    let customerInfo = [stateProvince, city, street, buildingName, roomNumber];
    return customerInfo.join('');
}

const caseUtils = {
    concatCustomerInfo
}

export default caseUtils;