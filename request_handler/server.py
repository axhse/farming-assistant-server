import socket
import json

from data_controller import DataController
from account_utils import AccountUtils
from crypto_utils import CryptoUtils
from os import environ
from dotenv import load_dotenv
from pyodbc import DatabaseError

load_dotenv()


class FarmingAppServer:

    HOST = environ['SERVER_HOST']
    PORT = int(environ['SERVER_PORT'])
    LISTENING_LIMIT = int(environ['LISTENING_LIMIT'])
    CLIENT_TIMEOUT = int(environ['CLIENT_TIMEOUT'])

    __aes_key = None
    __aes_iv = None

    @staticmethod
    def run():
        server = socket.socket(socket.AF_INET, socket.SOCK_STREAM, socket.IPPROTO_TCP)
        server.bind((FarmingAppServer.HOST, FarmingAppServer.PORT))
        server.listen(FarmingAppServer.LISTENING_LIMIT)
        while True:
            client, client_address = server.accept()
            client.settimeout(FarmingAppServer.CLIENT_TIMEOUT)
            FarmingAppServer.__handle_request(client)

    @staticmethod
    def __handle_request(client):
        try:
            rsa_key = client.recv(270)
            FarmingAppServer.__generate_new_aes()
            client.send(CryptoUtils.encrypt_rsa(FarmingAppServer.__aes_key, rsa_key))
            client.send(CryptoUtils.encrypt_rsa(FarmingAppServer.__aes_iv, rsa_key))
            token = FarmingAppServer.__decrypt_aes_and_decode(client.recv(64))
            request = json.loads(FarmingAppServer.__decrypt_aes_and_decode(client.recv(50000)))
            response = FarmingAppServer.__create_response(request, token)
            client.send(FarmingAppServer.__encode_and_encrypt_aes(response.to_json()))
        except:
            pass
        finally:
            client.close()

    @staticmethod
    def __create_response(request, token):
        try:
            if request['Type'] == 'SignUpRequest' or request['Type'] == 'SignInRequest':
                username = request['Username']
                password = request['Password']
                errors = []
                if not AccountUtils.username_format_is_correct(username):
                    errors.append('InvalidUsernameError')
                if not AccountUtils.password_format_is_correct(password):
                    errors.append('InvalidPasswordError')
                if len(errors):
                    return Response(errors=errors)

                pwd_hash = CryptoUtils.get_password_hash(password)

                if request['Type'] == 'SignUpRequest':
                    if DataController.user_exists(username):
                        return Response(errors='UserAlreadyExistsError')
                    DataController.add_user(username, AccountUtils.DEFAULT_CUSTOMER_INFO, pwd_hash)

                if request['Type'] == 'SignInRequest':
                    if not DataController.user_exists(username):
                        return Response(errors='UserNotFoundError')
                    if not DataController.verify_auth_by_pwd_hash(username, pwd_hash):
                        return Response(errors='PasswordAuthError')

                FarmingAppServer.__update_token(username)
                return Response(token=DataController.get_token(username))

            if not AccountUtils.token_format_is_correct(token):
                return Response(errors='TokenFormatError')
            username = DataController.get_username_by_token(token)
            response = Response()

            if request['Type'] == 'GetRecommendationsRequest':    # TODO: verify subscription
                if AccountUtils.field_is_correct(request['TargetField']):
                    response.Parameter = "[]"    # TODO: RecommendationMaker
                else:
                    response.Errors = ['InvalidFieldError']

            else:
                if username is None:
                    return Response(errors='TokenAuthError')

                FarmingAppServer.__update_token(username)
                response.NewAuthToken = DataController.get_token(username)

                if request['Type'] == 'GetCustomerInfoRequest':
                    response.Parameter = json.dumps(DataController.get_customer_info(username))

                if request['Type'] == 'UpdateCustomerInfoRequest':
                    if not AccountUtils.customer_info_is_correct(request['CustomerInfo']):
                        response.Errors = ['InvalidCustomerInfoError']
                    else:
                        DataController.update_customer_info(username, request['CustomerInfo'])

            return response

        except DatabaseError:
            return Response(errors='ServerError')
        except:
            return Response(errors='InvalidRequestError')

    @staticmethod
    def __generate_new_aes():
        FarmingAppServer.__aes_key = CryptoUtils.get_random_bytes(32)
        FarmingAppServer.__aes_iv = CryptoUtils.get_random_bytes(16)

    @staticmethod
    def __encode_and_encrypt_aes(data):
        return CryptoUtils.encrypt_aes(data.encode('utf-8'), FarmingAppServer.__aes_key, FarmingAppServer.__aes_iv)

    @staticmethod
    def __decrypt_aes_and_decode(data):
        return CryptoUtils.decrypt_aes(data, FarmingAppServer.__aes_key, FarmingAppServer.__aes_iv).decode('utf-8')

    @staticmethod
    def __update_token(username):
        token = DataController.get_token(username)
        if token is not None and DataController.check_token_relevance(token):
            return
        while True:
            new_token = CryptoUtils.generate_token()
            if not DataController.token_exists(new_token):
                DataController.add_token(username, new_token)
                return


class Response:
    def __init__(self, errors=None, parameter=None, token=None):
        if type(errors) == list:
            self.Errors = errors
        elif type(errors) == str:
            self.Errors = [errors]
        else:
            self.Errors = []
        self.Parameter = parameter
        self.NewAuthToken = token

    def to_json(self):
        return json.dumps(self.__dict__)


if __name__ == "__main__":
    FarmingAppServer.run()
