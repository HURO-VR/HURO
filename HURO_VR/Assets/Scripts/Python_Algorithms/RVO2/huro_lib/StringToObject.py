import json

class StringToObject:
    def __init__(self, json_str):
        data = json.loads(json_str)  # Convert JSON string to dictionary
        self._convert_dict_to_object(data)

    def _convert_dict_to_object(self, dictionary):
        for key, value in dictionary.items():
            if type(value) is dict:
                value = StringToObject(json.dumps(value))  # Recursively convert
            if (type(value) is list) and (len(value) > 0) and (type(value[0]) is dict):
                value = [StringToObject(json.dumps(item)) for item in value]  # Recursively convert
            setattr(self, key, value)

    def __repr__(self):
        return str(self.__dict__)
    