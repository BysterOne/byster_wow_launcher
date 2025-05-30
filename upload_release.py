import argparse
import logging
import sys
import re
from json import dumps
from urllib.parse import urlencode, unquote, urlparse, parse_qsl, ParseResult

import requests


def add_url_params(url, params):
    """ Add GET params to provided URL being aware of existing.

    :param url: string of target URL
    :param params: dict containing requested params to be added
    :return: string with updated URL

    >> url = 'https://stackoverflow.com/test?answers=true'
    >> new_params = {'answers': False, 'data': ['some','values']}
    >> add_url_params(url, new_params)
    'https://stackoverflow.com/test?data=some&data=values&answers=false'
    """
    # Unquoting URL first so we don't loose existing args
    url = unquote(url)
    # Extracting url info
    parsed_url = urlparse(url)
    # Extracting URL arguments from parsed URL
    get_args = parsed_url.query
    # Converting URL arguments to dict
    parsed_get_args = dict(parse_qsl(get_args))
    # Merging URL arguments dict with new params
    parsed_get_args.update(params)

    # Bool and Dict values should be converted to json-friendly values
    # you may throw this part away if you don't like it :)
    parsed_get_args.update(
        {k: dumps(v) for k, v in parsed_get_args.items()
         if isinstance(v, (bool, dict))}
    )

    # Converting URL argument to proper query string
    encoded_get_args = urlencode(parsed_get_args, doseq=True)
    # Creating new parsed result object based on provided with new
    # URL arguments. Same thing happens inside of urlparse.
    new_url = ParseResult(
        parsed_url.scheme, parsed_url.netloc, parsed_url.path,
        parsed_url.params, encoded_get_args, parsed_url.fragment
    ).geturl()

    return new_url


def argparser():
    parser = argparse.ArgumentParser()
    parser.add_argument(
        '-d', '--dll', type=str, required=True, help='DLL name for upload (Byster.Core.dll)')
    parser.add_argument(
        '-s', '--sha', type=str, required=True, help='GitLab commit SHA')
    parser.add_argument(
        '-g', '--gitlab', type=str, required=True, help='GitlabUser who triggered the pipeline')
    parser.add_argument(
        '-t', '--token', type=str, required=True, help='Gitlab Token for upload')
    parser.add_argument(
        '-u', '--url', type=str, required=True, help='Upload API url')
    parser.add_argument(
        '-b', '--branch', type=str, required=True, help='Branch')
    parser.add_argument(
        '-a', '--assembly', type=str, required=True, help='Assembly info file')
    parser.add_argument(
        '-v', '--verbose', action="store_true", help='Verbose')
    return parser


def log_verb(verbose, pid):
    if verbose:
        logging_level = logging.INFO
    else:
        logging_level = logging.ERROR
    logging.basicConfig(stream=sys.stderr, level=logging_level)
    return logging.getLogger(str(pid))


def main():
    args = argparser().parse_args()
    logger = log_verb(args.verbose, __name__)

    logger.info(args)
    gitlab_username = args.gitlab
    gitlab_token = args.token
    dll_name = args.dll
    commit_sha = args.sha
    base_url = args.url
    branch = args.branch
    assembly_file = args.assembly

    with open(f"./{assembly_file}", 'r', encoding="utf8") as f:
        assembly_info = f.read()
        version = re.search(
            r'AssemblyFileVersion\("([0-9\.]+)"\)', assembly_info
        ).group(1)

    params = {
        'gitlab': gitlab_username,
        'sha': commit_sha,
        'branch': branch,
        'version': version,
    }
    url = add_url_params(base_url, params)

    with open(f"./{dll_name}", 'rb') as f:
        data = f.read()

    headers = {'X-Gitlab-Token': gitlab_token}
    response = requests.post(url=url, headers=headers, data=data)

    if response.status_code > 400:
        raise Exception(response.text)


if __name__ == '__main__':
    main()
