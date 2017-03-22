class ObservationName(object):
    CAMI_WEIGHT = "CAMIWeight"
    CAMI_BLOOD_PRESSURE = "CAMIBloodPresure"

class QuestionnaireID(object):
    CAMI_WEIGHT     = 51
    CAMI_SATURATION = 52
    CAMI_BP         = 50

class MeasurementType(object):
    CAMI_BP         = "324.BP"
    CAMI_PULSE_OXY  = "328.SAT"
    CAMI_WEIGHT     = "326.WEIGHT"

class UnitCode(object):
    BPM         = "BPM"
    KILOGRAM    = "Kilo"
    MMHG        = "mmHg"
    MMOL        = "mmol/L"