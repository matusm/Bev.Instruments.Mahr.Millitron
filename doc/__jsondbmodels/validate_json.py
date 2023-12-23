import json
import jsonschema

def validate_json(json_data, schema):
    try:
        jsonschema.validate(instance=json_data, schema=schema)
        print("Validation successful.")
    except jsonschema.exceptions.ValidationError as e:
        print(f"Validation failed: {e}")

if __name__ == "__main__":
    # Load JSON data to validate
    with open("measurementModel_GBC_20230906-0519_OntologyMapping.json", "r") as json_file:
        data_to_validate = json.load(json_file)

    # Load JSON Schema
    with open("measurementModel_GBC_schema.json", "r") as schema_file:
        schema = json.load(schema_file)

    # Validate JSON data against the schema
    validate_json(data_to_validate, schema)