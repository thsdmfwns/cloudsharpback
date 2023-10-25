#!/bin/bash
echo "Check environment values..."

json_file="$TOOLPATH/EnvValues.json"
json_data=$(cat "${json_file}" | base64)

validate_result=0
validate_env_var() {
    local key=$(echo "$1" | base64 --decode)
    local pattern=$(echo "$2" | base64 --decode)
    local default=$(echo "$3" | base64 --decode)
    
    if [ -z "${!key}" ]; then
        if [ ${default} != "null" ]; then
            echo "Environment value ${key} is not set. Use default value ${default}"
            export "${key}"="${default}"
            return
        fi
        echo "Environment value ${key} is need to set."
        validate_result=1
        return
    fi

    if [ "${pattern}" == "null" ]; then
        return
    fi

    if [[ "${!key}" =~ ${pattern} ]]; then
        return
    fi

    echo "Environment value ${key}=${!key} is invalid value. for pattern ${pattern}"
    validate_result=1
    return
}

for row in $(echo "${json_data}" | base64 --decode | jq -r '.[] | @base64'); do
    _jq() {
        echo ${row} | base64 --decode | jq -r ${1} | base64
    }
    
    key=$(_jq '.Key')
    pattern=$(_jq '.Pattern')
    default=$(_jq '.Default')
        
    validate_env_var ${key} ${pattern} ${default}
done

echo "Done!"