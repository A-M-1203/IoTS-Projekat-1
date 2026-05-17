export const typeDefs = `#graphql
  scalar DateTime

  type SensorReading {
    id: ID!
    timestamp: DateTime!
    deviceId: String!
    location: String
    cropType: String
    season: String
    temperature: Float
    humidity: Float
    rainfall: Float
    soilMoisture: Float
    soilPh: Float
    lightIntensity: Float
    fertilizerUsed: Float
    irrigationNeeded: Boolean
    cropHealth: String
    yieldEstimate: Float
    pestRisk: String
    anomalyFlag: Boolean
  }

  input SensorReadingCreateInput {
    timestamp: DateTime!
    deviceId: String!
    location: String!
    cropType: String!
    season: String!
    temperature: Float!
    humidity: Float!
    rainfall: Float!
    soilMoisture: Float!
    soilPh: Float!
    lightIntensity: Float!
    fertilizerUsed: Float!
    irrigationNeeded: Boolean!
    cropHealth: String!
    yieldEstimate: Float!
    pestRisk: String!
    anomalyFlag: Boolean!
  }

  input SensorReadingUpdateInput {
    timestamp: DateTime
    deviceId: String
    location: String
    cropType: String
    season: String
    temperature: Float
    humidity: Float
    rainfall: Float
    soilMoisture: Float
    soilPh: Float
    lightIntensity: Float
    fertilizerUsed: Float
    irrigationNeeded: Boolean
    cropHealth: String
    yieldEstimate: Float
    pestRisk: String
    anomalyFlag: Boolean
  }

  type Query {
    sensorReadings(limit: Int = 100, offset: Int = 0): [SensorReading!]!
    sensorReading(id: ID!): SensorReading
  }

  type Mutation {
    createSensorReading(input: SensorReadingCreateInput!): SensorReading!
    updateSensorReading(id: ID!, input: SensorReadingUpdateInput!): SensorReading!
    deleteSensorReading(id: ID!): Boolean!
  }
`;
