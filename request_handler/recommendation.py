import json
from time import time
from weather import Weather


class RecommendationMaker:    # TODO

    relevance_periods = {'TestType': 100_000_000}

    @staticmethod
    def get_recommendations(target_field, recommendation_types=None):
        recommendation_makers = RecommendationMaker.__get_recommendation_makers()
        recommendations = []
        for recommendation_type in recommendation_types:
            weather = Weather.get_weather(recommendation_type, target_field['Location'])
            recommendation_value = recommendation_makers[recommendation_type](target_field, weather)
            relevance_limit_timestamp = round(time()) + RecommendationMaker.relevance_periods[recommendation_type]
            recommendations.append(Recommendation(recommendation_type, recommendation_value, relevance_limit_timestamp))
        return recommendations

    @staticmethod
    def get_test_type_recommendation_value(target_field, weather_info):
        return 'TestRecommendationValue'

    @staticmethod
    def __get_recommendation_makers():
        return {'TestType': RecommendationMaker.get_test_type_recommendation_value}


class Recommendation:

    def __init__(self, recommendation_type, value, relevance_limit_timestamp):
        self.Type = recommendation_type
        self.Value = value
        self.RelevanceLimitTimestamp = relevance_limit_timestamp

    def to_json(self):
        return json.dumps(self.__dict__)
