import json
import re


class AccountUtils:

    @staticmethod
    def customer_info_is_correct(customer_info, username):
        customer_info_keys = ['Username', 'Fields']
        if not AccountUtils.verify_json(customer_info, customer_info_keys):
            return False
        if not AccountUtils.username_format_is_correct(customer_info['Username'])\
                or customer_info['Username'] != username:
            return False
        if type(customer_info['Fields']) is not list:
            return False
        for field in customer_info['Fields']:
            if not AccountUtils.field_is_correct(field):
                return False
        return True

    @staticmethod
    def field_is_correct(field):
        field_keys = ['Location', 'CultivatedPlant', 'Name']
        cultivated_plants = []
        if not AccountUtils.verify_json(field, field_keys):
            return False
        if type(field['Location']) is not str:   # TODO: location regex check
            return False
        if type(field['CultivatedPlant']) is not str or field['CultivatedPlant'] not in cultivated_plants:
            return False
        if type(field['Name']) is not str or len(field['Name']) > 30:
            return False
        return True

    @staticmethod
    def verify_json(data, key_list, other_keys_forbidden=True):
        if not type(data) is dict:
            return False
        for key in key_list:
            if key not in data.keys():
                return False
        if other_keys_forbidden:
            for key in data.keys():
                if key not in key_list:
                    return False
        return True

    @staticmethod
    def username_format_is_correct(data):
        if type(data) is not str:
            return False
        return 6 <= len(data) <= 20 and re.search(r'[^a-z0-9]', data) is None

    @staticmethod
    def password_format_is_correct(data):
        if type(data) is not str:
            return False
        return 8 <= len(data) <= 30  # and re.search(r'[^A-Za-z0-9]', data) is None

    @staticmethod
    def token_format_is_correct(data):
        if type(data) is not str:
            return False
        return len(data) <= 64 and re.search(r'[^a-z0-9]', data) is None

    @staticmethod
    def get_default_customer_info(username):
        info = {'Username': username, 'Fields': []}
        return json.dumps(info)
