import configparser

def read_config_file(file_path):
    config = configparser.ConfigParser()
    config.read(file_path)

    # Loop through sections
    for section in config.sections():
        print(f"[{section}]")
        # Loop through key-value pairs in each section
        for key, value in config.items(section):
            print(f"{key} = {value}")
        print()  # Print an empty line between sections

if __name__ == "__main__":
    config_file_path = "terminology.config"
    read_config_file(config_file_path)