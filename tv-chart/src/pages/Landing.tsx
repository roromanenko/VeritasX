import FuzzyText from "../reactbits/TextAnimations/FuzzyText/FuzzyText"

export const Landing = () => {
    return (
        <>
            <header>
                <div className="brand-container">
                    <FuzzyText
                        hoverIntensity={0.4}
                        color="#00d4aa"
                        fontWeight={900}
                        >
                        VeritasX</FuzzyText>
                </div>
            </header>
            <div className="brand-container">
                <FuzzyText
                    fontSize={"2rem"}
                    hoverIntensity={0.2}
                    baseIntensity={0.05}
                    color="rgba(224, 230, 237, 0.8)"
                    fontWeight={900}
                >
                    BUILD AND TEST YOUR TECHICAL IDEAS</FuzzyText>
            </div>
        </>
    )
}
