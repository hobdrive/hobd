<?xml version="1.0"?>
<xsl:stylesheet
     version="1.0"
     xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:output indent="no" method="text" encoding="utf-8"/>

<xsl:param name='ns' select='/parameters/@namespace'/>
<xsl:param name='nameprefix'  select='"sname."'/>
<xsl:param name='descrprefix' select='"sdesc."'/>
<xsl:param name='lang'/>

<xsl:template match="/files">

<xsl:apply-templates select='ecu'/>

</xsl:template>

<xsl:template match='ecu'>
  <xsl:apply-templates select='document(text())/parameters'/>
</xsl:template>

<xsl:template match="parameters">
  <xsl:apply-templates select='parameter'/>
</xsl:template>

<xsl:template match='parameter'>
  <xsl:if test="description[@lang = $lang]">
    <xsl:apply-templates select='description[@lang = $lang]'/>
  </xsl:if>
  <xsl:if test="not(description[@lang = $lang]) and $lang = 'en'">
    <xsl:apply-templates select='description[not(@lang)]'/>
  </xsl:if>
</xsl:template>


<xsl:template match='description'>
  <xsl:value-of select='$nameprefix'/><xsl:value-of select='../@id'/>  = <xsl:value-of select='name'/>
  <xsl:text>&#10;</xsl:text>
  <xsl:value-of select='$descrprefix'/><xsl:value-of select='../@id'/> = <xsl:value-of select='description'/>
  <xsl:text>&#10;</xsl:text>
  <xsl:text>&#10;</xsl:text>
</xsl:template>

</xsl:stylesheet>
