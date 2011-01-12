<?xml version="1.0"?>
<xsl:stylesheet
     version="1.0"
     xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:output indent="yes" method="xml" encoding="utf-8"/>

<xsl:template match="/files">
<html>
<head>
</head>
<body>

<xsl:apply-templates select='ecu' mode='toc'/>
<xsl:apply-templates select='ecu'/>

</body>
</html>
</xsl:template>

<xsl:template match='ecu' mode='toc'>
  <xsl:apply-templates select='document(text())/parameters' mode='toc'/>
</xsl:template>

<xsl:template match='parameters' mode='toc'>
  <xsl:param name='ns' select='@namespace'/>
  <h1>
  <xsl:value-of select='$ns'/> Sensors
  </h1>
  <xsl:apply-templates select='parameter' mode='toc'/>
</xsl:template>

<xsl:template match='parameter' mode='toc'>
  <a href='#{@id}'><xsl:value-of select='@id'/></a>
  <br/>
</xsl:template>

<xsl:template match='ecu'>
  <xsl:apply-templates select='document(text())/parameters'/>
</xsl:template>

<xsl:template match='parameters'>
  <xsl:param name='ns' select='@namespace'/>
  <xsl:param name='class' select='concat($ns, "Sensors")'/>
  <h1>
  <xsl:value-of select='$ns'/> Sensors
  </h1>
  <h3>
  <xsl:value-of select='@description'/>
  </h3>

  <div class='parameters'>
  <xsl:apply-templates select='parameter'/>
  </div>
</xsl:template>

<xsl:template match='parameter[address]'>
  <h3 id="{@id}">
  <xsl:value-of select='@id'/>
  </h3>
  <h4>
  <xsl:apply-templates select='description'/>
  </h4>
  <p>
  Address: <xsl:value-of select='address/byte'/>
  </p>
  <p>
  Formula: <xsl:value-of select='normalize-space(value)'/>
  </p>
</xsl:template>

<xsl:template match='parameter[class]'>
  <h3 id="{@id}">
  <xsl:value-of select='@id'/>
  </h3>
  <div class='description'>
  <xsl:apply-templates select='description'/>
  </div>
  <p>
  Class: <xsl:value-of select='class'/>
  </p>
  <p>
  Properties:
  <xsl:apply-templates select='property'/>
  </p>
</xsl:template>

<xsl:template match='property'>
  
  <xsl:value-of select='@name'/> = <xsl:value-of select='text()'/>

</xsl:template>

<xsl:template match='parameter/description'>
  Name: <xsl:value-of select='name'/> (<xsl:value-of select='@lang'/>)
  <br/>
  Description: <xsl:value-of select='description'/> (<xsl:value-of select='@lang'/>)
  <br/>
  <xsl:if test='unit'>
    Units: <xsl:value-of select='unit'/> (<xsl:value-of select='@lang'/>)
    <br/>
  </xsl:if>
</xsl:template>

</xsl:stylesheet>
